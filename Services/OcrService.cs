using System.Text;
using ImageMagick;
using Microsoft.Extensions.Options;
using OcrWorkerDemo.Models;
using Tesseract;

namespace OcrWorkerDemo.Services
{
    /// <summary>
    /// OCR pipeline: PDF → Magick.NET (Ghostscript) → preprocessing → Tesseract.
    /// </summary>
    public class OcrService
    {
        private readonly OcrOptions _defaults;
        private readonly string _tessDataPath;
        private readonly ILogger<OcrService> _logger;

        public OcrService(IOptions<OcrOptions> options, IWebHostEnvironment env, ILogger<OcrService> logger)
        {
            _defaults = options.Value;
            _tessDataPath = Path.Combine(env.ContentRootPath, "tessdata");
            _logger = logger;
            Directory.CreateDirectory(_tessDataPath);
        }

        public OcrTweaks DefaultTweaks() => new(
            Language: _defaults.DefaultLanguage,
            EngineMode: Enum.Parse<EngineMode>(_defaults.DefaultOem, true),
            PageSegMode: Enum.Parse<PageSegMode>(_defaults.DefaultPsm, true),
            Dpi: _defaults.DefaultDpi,
            UseDeskew: _defaults.UseDeskew,
            UseAdaptiveThreshold: _defaults.UseAdaptiveThreshold,
            UseSharpen: _defaults.UseSharpen,
            MaxPages: _defaults.MaxPages
        );

        public async Task<OcrResult> ProcessPdfAsync(Stream pdfStream, OcrTweaks tweaks, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Starting OCR (Lang={Lang}, DPI={Dpi}, Pages={Pages})", tweaks.Language, tweaks.Dpi, tweaks.MaxPages);

                var (pageImages, thumbs, fulls) = await ConvertPdfToOcrImagesAsync(pdfStream, tweaks, ct);

                var pages = new List<OcrPageResult>();
                var infos = new List<OcrPageInfo>();

                using var engine = new TesseractEngine(_tessDataPath, tweaks.Language, tweaks.EngineMode)
                {
                    DefaultPageSegMode = tweaks.PageSegMode
                };

                for (int i = 0; i < pageImages.Count; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    using var png = pageImages[i];
                    using var ms = new MemoryStream();
                    png.Write(ms, MagickFormat.Png);
                    using var pix = Pix.LoadFromMemory(ms.ToArray());

                    using var page = engine.Process(pix);
                    var text = page.GetText() ?? string.Empty;
                    var conf = page.GetMeanConfidence();

                    pages.Add(new OcrPageResult(i + 1, text, conf));
                    infos.Add(new OcrPageInfo(i + 1, conf, thumbs[i], fulls[i]));

                    _logger.LogDebug("Processed page {PageIndex} with confidence {Confidence:P1}", i + 1, conf);
                }

                _logger.LogInformation("OCR completed successfully for {Count} pages", pages.Count);

                var combined = new StringBuilder();
                foreach (var p in pages)
                {
                    combined.AppendLine($"--- Page {p.PageIndex} (conf {p.MeanConfidence:P0}) ---");
                    combined.AppendLine(p.Text);
                    combined.AppendLine();
                }

                return new OcrResult(pages, combined.ToString(), infos);
            }
            catch (MagickException ex)
            {
                _logger.LogError(ex, "ImageMagick failed to process the PDF.");
                throw new InvalidOperationException("Failed to convert PDF for OCR. Check Ghostscript installation.", ex);
            }
            catch (TesseractException ex)
            {
                _logger.LogError(ex, "Tesseract failed during OCR.");
                throw new InvalidOperationException("Tesseract OCR failed. Verify tessdata files and language codes.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during OCR processing.");
                throw;
            }
        }

        private async Task<(List<MagickImage> Images, List<string> Thumbs, List<string> Fulls)>
            ConvertPdfToOcrImagesAsync(Stream pdfStream, OcrTweaks t, CancellationToken ct)
        {
            var images = new List<MagickImage>();
            var thumbs = new List<string>();
            var fulls = new List<string>();

            try
            {
                var settings = new MagickReadSettings
                {
                    Density = new Density(t.Dpi, t.Dpi),
                    ColorSpace = ColorSpace.Gray
                };
                settings.SetDefine(MagickFormat.Pdf, "use-cropbox", "true");

                using var collection = new MagickImageCollection();
                using var buffer = new MemoryStream();
                await pdfStream.CopyToAsync(buffer, ct);
                buffer.Position = 0;

                collection.Read(buffer.ToArray(), settings);

                var pageCount = Math.Min(collection.Count, Math.Max(1, t.MaxPages));
                for (int i = 0; i < pageCount; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    using var frame = (MagickImage)collection[i].Clone();

                    if (t.UseDeskew) frame.Deskew(new Percentage(40));
                    frame.ContrastStretch(new Percentage(0.1), new Percentage(0.1));
                    if (t.UseSharpen) frame.AdaptiveSharpen(1, 1);

                    if (t.UseAdaptiveThreshold)
                        frame.AdaptiveThreshold(15, 15, 5);
                    else
                    {
                        frame.ReduceNoise();
                        frame.BlackThreshold(new Percentage(50));
                        frame.WhiteThreshold(new Percentage(50));
                    }

                    frame.Format = MagickFormat.Png;
                    frame.Density = new Density(t.Dpi);

                    using var full = (MagickImage)frame.Clone();
                    if (full.Width > 1200) full.Resize(1200, 0);
                    using var fms = new MemoryStream();
                    full.Write(fms, MagickFormat.Png);
                    fulls.Add($"data:image/png;base64,{Convert.ToBase64String(fms.ToArray())}");

                    using var thumb = (MagickImage)frame.Clone();
                    thumb.Resize(new MagickGeometry("300x"));
                    using var tms = new MemoryStream();
                    thumb.Write(tms, MagickFormat.Png);
                    thumbs.Add($"data:image/png;base64,{Convert.ToBase64String(tms.ToArray())}");

                    images.Add((MagickImage)frame.Clone());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert PDF to images.");
                throw;
            }

            return (images, thumbs, fulls);
        }
    }
}

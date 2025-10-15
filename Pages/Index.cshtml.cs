using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcrWorkerDemo.Models;
using OcrWorkerDemo.Services;
using Tesseract;

namespace OcrWorkerDemo.Pages;

/// <summary>
/// Razor PageModel: binds form inputs, applies presets, invokes OCR, exposes results to the view.
/// </summary>
public class IndexModel : PageModel
{
    private readonly OcrService _ocr;
    private readonly ILogger<IndexModel> _logger;

    public List<OcrPageInfo> PageInfos { get; private set; } = new();
    public List<OcrPageResult> PageResults { get; private set; } = new();

    public IndexModel(OcrService ocr, ILogger<IndexModel> logger)
    {
        _ocr = ocr;
        _logger = logger;
        Tweaks = _ocr.DefaultTweaks();
        Preset = "Default";
    }

    [BindProperty] public IFormFile? Pdf { get; set; }
    [BindProperty] public int Dpi { get; set; }
    [BindProperty] public int MaxPages { get; set; }
    [BindProperty] public string Language { get; set; } = "";
    [BindProperty] public string Oem { get; set; } = "LstmOnly";
    [BindProperty] public string Psm { get; set; } = "Auto";
    [BindProperty] public bool UseDeskew { get; set; }
    [BindProperty] public bool UseAdaptiveThreshold { get; set; }
    [BindProperty] public bool UseSharpen { get; set; }

    [BindProperty] public string Preset { get; set; } = "Default";

    public OcrTweaks Tweaks { get; private set; }
    public string? ResultText { get; private set; }

    public void OnGet()
    {
        Tweaks = _ocr.DefaultTweaks();
        ApplyTweaksToBindings(Tweaks);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Pdf == null || Pdf.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Please select a PDF file.");
            return Page();
        }

        try
        {
            var def = _ocr.DefaultTweaks();
            var tweaks = new OcrTweaks(
                Language: string.IsNullOrWhiteSpace(Language) ? def.Language : Language,
                EngineMode: Enum.Parse<EngineMode>(string.IsNullOrWhiteSpace(Oem) ? def.EngineMode.ToString() : Oem, true),
                PageSegMode: Enum.Parse<PageSegMode>(string.IsNullOrWhiteSpace(Psm) ? def.PageSegMode.ToString() : Psm, true),
                Dpi: Dpi <= 0 ? def.Dpi : Dpi,
                UseDeskew: UseDeskew,
                UseAdaptiveThreshold: UseAdaptiveThreshold,
                UseSharpen: UseSharpen,
                MaxPages: Math.Clamp(MaxPages <= 0 ? def.MaxPages : MaxPages, 1, 200)
            );

            if (Enum.TryParse<OcrPreset>(Preset, out var selected))
                tweaks = OcrPresets.Apply(tweaks, selected);
            ApplyTweaksToBindings(tweaks);

            using var stream = Pdf.OpenReadStream();
            var result = await _ocr.ProcessPdfAsync(stream, tweaks, HttpContext.RequestAborted);

            ResultText = result.CombinedText;
            PageInfos = result.PageInfos;
            PageResults = result.Pages;
            Tweaks = tweaks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR processing failed");
            ModelState.AddModelError(string.Empty, $"OCR failed: {ex.Message}");
        }

        return Page();
    }

    private void ApplyTweaksToBindings(OcrTweaks t)
    {
        Dpi = t.Dpi;
        MaxPages = t.MaxPages;
        Language = t.Language;
        Oem = t.EngineMode.ToString();
        Psm = t.PageSegMode.ToString();
        UseDeskew = t.UseDeskew;
        UseAdaptiveThreshold = t.UseAdaptiveThreshold;
        UseSharpen = t.UseSharpen;
    }
}

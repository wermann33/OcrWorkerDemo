namespace OcrWorkerDemo.Models
{
    /// <summary>
    /// Default OCR configuration loaded from appsettings.json ("Ocr" section).
    /// </summary>
    public class OcrOptions
    {
        /// <summary>Language codes used by Tesseract (e.g. "deu", "eng", "deu+eng").</summary>
        public string DefaultLanguage { get; set; } = "deu+eng";

        /// <summary>OCR engine mode, mapped to Tesseract.EngineMode.</summary>
        public string DefaultOem { get; set; } = "LstmOnly";

        /// <summary>Page segmentation mode, mapped to Tesseract.PageSegMode.</summary>
        public string DefaultPsm { get; set; } = "Auto";

        /// <summary>Rasterization DPI for PDF pages (300–400 is typical).</summary>
        public int DefaultDpi { get; set; } = 300;

        /// <summary>Enables deskewing of scanned pages.</summary>
        public bool UseDeskew { get; set; } = true;

        /// <summary>Enables adaptive thresholding for uneven lighting.</summary>
        public bool UseAdaptiveThreshold { get; set; } = true;

        /// <summary>Applies light sharpening to improve edge contrast.</summary>
        public bool UseSharpen { get; set; } = true;

        /// <summary>Maximum number of pages processed per document.</summary>
        public int MaxPages { get; set; } = 10;
    }
}

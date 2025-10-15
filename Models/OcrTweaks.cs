using Tesseract;

namespace OcrWorkerDemo.Models
{
    /// <summary>
    /// Runtime OCR parameters adjusted via the UI.
    /// </summary>
    public record OcrTweaks(
        string Language,
        EngineMode EngineMode,
        PageSegMode PageSegMode,
        int Dpi,
        bool UseDeskew,
        bool UseAdaptiveThreshold,
        bool UseSharpen,
        int MaxPages
    );
}

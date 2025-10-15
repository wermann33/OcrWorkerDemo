using OcrWorkerDemo.Models;
using Tesseract;

/// <summary>
/// Predefined OCR configuration profiles used by the UI presets.
/// </summary>
public enum OcrPreset
{
    Default,
    LowQualityScan,
    Photo,
    Form
}

/// <summary>
/// Provides preset configurations for common OCR scenarios.
/// </summary>
public static class OcrPresets
{
    public static OcrTweaks Apply(OcrTweaks t, OcrPreset preset) => preset switch
    {
        OcrPreset.LowQualityScan => t with
        {
            Dpi = 400,
            UseDeskew = true,
            UseAdaptiveThreshold = true,
            UseSharpen = true,
            PageSegMode = PageSegMode.SingleColumn
        },

        OcrPreset.Photo => t with
        {
            Dpi = 500,
            UseDeskew = true,
            UseAdaptiveThreshold = true,
            UseSharpen = true,
            PageSegMode = PageSegMode.Auto
        },

        OcrPreset.Form => t with
        {
            Dpi = 300,
            UseDeskew = true,
            UseAdaptiveThreshold = false,
            UseSharpen = false,
            PageSegMode = PageSegMode.SparseText
        },

        _ => t
    };
}

namespace OcrWorkerDemo.Models
{
    /// <summary>
    /// Basic information about a processed page, including confidence and preview images.
    /// </summary>
    public record OcrPageInfo(
        int PageIndex,
        float MeanConfidence,
        string? ThumbDataUrl,
        string? FullDataUrl
    );
}

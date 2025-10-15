namespace OcrWorkerDemo.Models
{
    /// <summary>
    /// OCR output for a single page, including recognized text and confidence.
    /// </summary>
    public record OcrPageResult(
        int PageIndex,
        string Text,
        float MeanConfidence
    );
}

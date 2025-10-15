namespace OcrWorkerDemo.Models
{
    /// <summary>
    /// Aggregated OCR result containing all pages, combined text, and page metadata.
    /// </summary>
    public record OcrResult(
        List<OcrPageResult> Pages,
        string CombinedText,
        List<OcrPageInfo> PageInfos
    );
}

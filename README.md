# OCR Worker Demo (.NET 8)

Minimal Razor Pages app demonstrating a clean OCR pipeline:  
**PDF → Magick.NET (Ghostscript) → Tesseract → text + confidence + previews**

---

## Prerequisites

- **.NET SDK 8.0**   
- **Ghostscript** (for PDF rasterization)
  - Windows: [ghostscript.com](https://ghostscript.com/)
  - macOS: `brew install ghostscript`
  - Linux (Debian/Ubuntu): `sudo apt-get install ghostscript`
- **Tesseract traineddata files** in the local `tessdata/` directory  
  - Download from the official repositories:  
    - Standard: [tessdata](https://github.com/tesseract-ocr/tessdata)  
    - High accuracy: [tessdata_best](https://github.com/tesseract-ocr/tessdata_best)  
    - High speed: [tessdata_fast](https://github.com/tesseract-ocr/tessdata_fast)
  - Combine multiple languages using `+`, e.g. `deu+eng`


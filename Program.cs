using ImageMagick;
using OcrWorkerDemo.Models;
using OcrWorkerDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Optional Ghostscript directory (only needed if Ghostscript isn't on PATH)
var gsDir = builder.Configuration["Ghostscript:Directory"];
if (!string.IsNullOrWhiteSpace(gsDir))
{
    MagickNET.SetGhostscriptDirectory(gsDir);
}

builder.Services.AddRazorPages();
builder.Services.AddSingleton<OcrService>();
builder.Services.Configure<OcrOptions>(builder.Configuration.GetSection("Ocr"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.Run();

using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

string imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "generated-images");
if (!Directory.Exists(imagesPath))
{
    Directory.CreateDirectory(imagesPath);
}

// === CONFIGURAZIONE IMMAGINI ===
string imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "generated-images");
if (!Directory.Exists(imagesPath)) Directory.CreateDirectory(imagesPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/images"
    // Risultato: http://localhost:8080/images/abc-123_0.jpg funziona.
    // http://localhost:8080/images/ dà 404 (giusto così).
});

Console.WriteLine($"[INFO] Servendo immagini da: {imagesPath}");

app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

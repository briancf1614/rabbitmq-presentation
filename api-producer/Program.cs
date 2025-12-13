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

// Creiamo un provider per non ripetere il codice
var fileProvider = new PhysicalFileProvider(imagesPath);
var requestPath = "/images";

// 2. ABILITA LA VISIONE DEI FILE (Se conosci il nome esatto)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = fileProvider,
    RequestPath = requestPath
});

// 3. ABILITA L'ELENCO DEI FILE (Quello che vuoi tu!)
// Ora se vai su /images vedrai la lista HTML dei file
app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = fileProvider,
    RequestPath = requestPath
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

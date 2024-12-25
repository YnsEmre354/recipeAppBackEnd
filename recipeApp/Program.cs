using Microsoft.EntityFrameworkCore;
using recipeApp.Model;

var builder = WebApplication.CreateBuilder(args);

// CORS politikası ekleme
builder.Services.AddCors(options =>
{
    // CORS politikası adı veriyoruz
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()  // Tüm domainlere izin verir
               .AllowAnyMethod()  // GET, POST vb. tüm HTTP metodlarına izin verir
               .AllowAnyHeader(); // Tüm HTTP başlıklarına izin verir
    });
});

// Geliştirme ortamında SSL sertifikasını geçersiz kılma
if (builder.Environment.IsDevelopment())
{
    // Https yönlendirmesi için port belirleme
    builder.Services.AddHttpsRedirection(options =>
    {
        options.HttpsPort = 5001; // HTTPS bağlantısı için port numarası
    });

    // SSL sertifikası doğrulamasını geçersiz kılma (geliştirme için)
    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// API için gerekli servislerin eklenmesi
builder.Services.AddControllers();

// Swagger API dökümantasyonu
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// CORS'i uygulamaya ekliyoruz
app.UseCors("AllowAll");

// Geliştirme ortamında Swagger'ı etkinleştirme
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS yönlendirmesini etkinleştirme
app.UseHttpsRedirection();

// Yetkilendirme middleware'ini etkinleştirme
app.UseAuthorization();

// API controller'larını tanımlama
app.MapControllers();

// Uygulamayı başlatma
app.Run();

using EtkinlikSistemiAPI;
using EtkinlikSistemiAPI.Data;
using EtkinlikSistemiAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Veritabanı bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 🛠️ JSON seçeneklerini burada ayarladık (tek AddControllers çağrısı ile)  burayı sonrafan ekledin bak sonra
builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve; // 🔁 Döngüsel referansları destekle
        x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

    });

builder.Services.AddHttpClient<WeatherService>();// hava durumu için

builder.Services.AddEndpointsApiExplorer();
// Controller ve Swagger
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

  AdminSeeder.SeedAdmin(app);//admin için 
// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//app.UseHttpsRedirection();
app.UseCors("AllowAll");
//app.UseCors("AllowFrontend");
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();
app.Run();

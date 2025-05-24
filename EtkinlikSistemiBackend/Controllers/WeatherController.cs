using EtkinlikSistemiAPI.Data;
using EtkinlikSistemiAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtkinlikSistemiAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly WeatherService _weatherService;

        public WeatherController(AppDbContext context, WeatherService weatherService)
        {
            _context = context;
            _weatherService = weatherService;
        }

        // Belirli bir şehir için hava durumu bilgisini döner//////////////////////////////////////
        [HttpGet("{city}")]
        public async Task<IActionResult> GetWeather(string city)
        {
            var result = await _weatherService.GetWeatherConditionAsync(city);  // Şehrin hava durumu bilgisini al
            return Ok(new { City = city, Weather = result });// Şehre ait hava durumunu döndür
        }

        // Veritabanındaki tüm etkinlikler için hava durumunu günceller///////////////////////
        [HttpPut("guncelle-hava-durumu")]
        public async Task<IActionResult> GuncelleHavaDurumu()
        {
            var etkinlikler = _context.Etkinlikler.ToList();// Tüm etkinlikleri veritabanından al


            // Her etkinlik için hava durumunu al ve uygunluk kontrolü yap
            foreach (var etkinlik in etkinlikler)
            {
                // Etkinliğin lokasyonu ve tarihiyle hava durumu açıklamasını al
                var description = await _weatherService.GetWeatherDescriptionAsync(etkinlik.Lokasyon, etkinlik.Tarih);
                etkinlik.HavaDurumu = description; // Etkinliğin hava durumu bilgisini güncelle
                 // Etkinlik planlanabilir mi bilgisini ayarla
                etkinlik.PlanlanabilirMi = _weatherService.IsEventPlanSuitable(description);
            }

            await _context.SaveChangesAsync(); // Değişiklikleri veritabanına kaydet
            return Ok("Hava durumu başarıyla güncellendi.");
        }
    }
}

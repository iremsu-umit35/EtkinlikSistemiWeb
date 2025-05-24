using EtkinlikSistemiAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EtkinlikSistemiAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Sadece giriş yapan kullanıcılar erişebilir
    public class TicketController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketController(AppDbContext context)
        {
            _context = context;
        }
       /* 
        // 1. Giriş yapan kullanıcının satın aldığı biletleri getir
        [HttpGet("kullanici")]
        public IActionResult KullaniciBiletleri()
        {
            // JWT'den kullanıcı ID'sini al
            var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(kullaniciIdString) || !int.TryParse(kullaniciIdString, out int kullaniciId))
            {
                return Unauthorized("Kullanıcı doğrulanamadı.");
            }

            // Biletleri çek
            var biletler = _context.Biletler
                .Include(b => b.Etkinlik)
                .Where(b => b.KullaniciId == kullaniciId && b.OdendiMi)
                .Select(b => new
                {
                    BiletId = b.Id,
                    EtkinlikAdi = b.Etkinlik.Ad,
                    Tarih = b.Etkinlik.Tarih,
                    Konum = b.Etkinlik.Lokasyon,
                    AlimTarihi = b.AlimTarihi,
                    Fiyat = b.Fiyat
                })
                .ToList();

            if (!biletler.Any())
            {
                return NotFound("Henüz satın alınmış bilet bulunmuyor.");
            }

            return Ok(biletler);
        }
       */
    }
}
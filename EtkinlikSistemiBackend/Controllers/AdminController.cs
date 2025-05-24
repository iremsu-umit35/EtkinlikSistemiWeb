using EtkinlikSistemiAPI.Data;
using EtkinlikSistemiAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace EtkinlikSistemiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Yeni etkinlik oluştur
        [HttpPost("etkinlik-ekle")]
        public async Task<IActionResult> EtkinlikEkle([FromBody] Event etkinlik)
        {
            // Kalan kapasite başlangıçta toplam kapasiteye eşit
            etkinlik.KalanKapasite = etkinlik.Kapasite;

            _context.Etkinlikler.Add(etkinlik);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Etkinlik başarıyla eklendi.", id = etkinlik.Id });
        }



        // 2. Etkinlik güncelle (id ile)
        [HttpPut("etkinlik-guncelle/{id}")]
        public async Task<IActionResult> EtkinlikGuncelle(int id, [FromBody] Event guncelEtkinlik)
        {
            var etkinlik = await _context.Etkinlikler.FindAsync(id);
            if (etkinlik == null)
                return NotFound("Etkinlik bulunamadı.");

            // Kapasite değişirse KalanKapasite'yi güncelle
            if (etkinlik.Kapasite != guncelEtkinlik.Kapasite)
            {
                var fark = guncelEtkinlik.Kapasite - etkinlik.Kapasite;
                etkinlik.KalanKapasite += fark;
            }

            etkinlik.Ad = guncelEtkinlik.Ad;
            etkinlik.Aciklama = guncelEtkinlik.Aciklama;
            etkinlik.Tur = guncelEtkinlik.Tur;
            etkinlik.Tarih = guncelEtkinlik.Tarih;
            etkinlik.Kapasite = guncelEtkinlik.Kapasite;
            etkinlik.Lokasyon = guncelEtkinlik.Lokasyon;
            etkinlik.BiletFiyati = guncelEtkinlik.BiletFiyati;

            await _context.SaveChangesAsync();
            return Ok("Etkinlik başarıyla güncellendi.");
        }

        // 3. Etkinlik sil
        [HttpDelete("etkinlik-sil/{id}")]
        public IActionResult EtkinlikSil(int id)
        {
            var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.Id == id);
            if (etkinlik == null)
                return NotFound("Etkinlik bulunamadı.");

            _context.Etkinlikler.Remove(etkinlik);
            _context.SaveChanges();
            return Ok("Etkinlik başarıyla silindi.");
        }

        //  4. Tüm etkinlikleri listele
        [HttpGet("etkinlikleri")]
        public IActionResult TumEtkinlikleriGetir()
        {
            var etkinlikler = _context.Etkinlikler.OrderBy(e => e.Tarih).ToList();
            return Ok(etkinlikler);
        }



        // Kullanıcıyı onaylama
        [HttpPut("onayla/{id}")]
        public async Task<IActionResult> KullaniciOnayla(int id)
        {
            var user = await _context.Kullanicilar.FindAsync(id);

            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            if (user.Onaylandi)
                return BadRequest(new { message = "Kullanıcı zaten onaylanmış." });

            user.Onaylandi = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kullanıcı başarıyla onaylandı." });
        }

        // Onay bekleyen kullanıcıları listeleme
        [HttpGet("onaysizlar")]
        public async Task<IActionResult> OnaysizKullanicilariListele()
        {
            var users = await _context.Kullanicilar
                .Where(u => !u.Onaylandi && u.Rol == "kullanici")
                .ToListAsync();

            return Ok(users);
        }



        //duyuru işlemleri için
        [HttpPost("duyuru-ekle")]
        public async Task<IActionResult> DuyuruEkle([FromBody] announcement duyuru)
        {
            if (duyuru == null || string.IsNullOrWhiteSpace(duyuru.Baslik) || string.IsNullOrWhiteSpace(duyuru.Icerik))
            {
                return BadRequest("Geçersiz duyuru verisi.");
            }

            duyuru.Tarih = DateTime.Now;
            _context.Announcements.Add(duyuru);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Duyuru başarıyla eklendi." });
        }

        [HttpPut("duyuru-yayinla/{id}")]
        public async Task<IActionResult> TogglePublishStatus(int id, [FromQuery] bool isPublished)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                return NotFound("Duyuru bulunamadı.");
            }

            announcement.IsPublished = isPublished;
            await _context.SaveChangesAsync();

            return Ok(new { message = isPublished ? "Duyuru yayınlandı." : "Duyuru yayından kaldırıldı." });
        }


        //duyuruları getirme
        [HttpGet("duyurular")]
        public async Task<IActionResult> GetAnnouncements()
        {
            var announcements = await _context.Announcements.ToListAsync();
            return Ok(announcements);
        }

        //Sadece yayında olan duyurular
        [HttpGet("yayindaki-duyurular")]
        public async Task<IActionResult> GetPublishedAnnouncements()
        {
            var duyurular = await _context.Announcements
                .Where(d => d.IsPublished)
                .ToListAsync();

            return Ok(duyurular);
        }
    }

}

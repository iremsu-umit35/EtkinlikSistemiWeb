using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using EtkinlikSistemiAPI.Data;
using EtkinlikSistemiAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EtkinlikSistemiAPI.Controllers
{
    //[Authorize(Roles = "User")]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // Sepete ürün (etkinlik) ekleme/////////////////////////////////////
        [HttpPost("ekle")]
        public async Task<IActionResult> SepeteEkle(int kullaniciId, int etkinlikId, int adet)
        {
            // 1. Kontroller Etkinliğin var olup olmadığını kontrol et
            var etkinlik = await _context.Etkinlikler.FindAsync(etkinlikId);
            if (etkinlik == null) return BadRequest("Etkinlik bulunamadı");

            // 2. Sepeti bul veya oluştur
            var sepet = await _context.Sepetler
                .Include(s => s.SepettekiEtkinlikler)
                .FirstOrDefaultAsync(s => s.KullaniciId == kullaniciId);

            if (sepet == null)
            {
                sepet = new Cart { KullaniciId = kullaniciId };
                _context.Sepetler.Add(sepet);
                await _context.SaveChangesAsync();
            }

            // 3.Sepette aynı etkinlik varsa miktarını artır, yoksa yeni ürün olarak ekle
            var sepettekiUrun = sepet.SepettekiEtkinlikler
                .FirstOrDefault(x => x.EtkinlikId == etkinlikId);

            if (sepettekiUrun != null)
            {
                sepettekiUrun.Adet += adet; // Miktarı artır
                sepettekiUrun.ToplamFiyat = (int)(sepettekiUrun.Adet * etkinlik.BiletFiyati);
            }
            else
            {
                sepet.SepettekiEtkinlikler.Add(new CartEvent
                {
                    EtkinlikId = etkinlikId,
                    Adet = adet,
                    ToplamFiyat = (int)(adet * etkinlik.BiletFiyati)
                });
            }

            // 4.Değişiklikleri veritabanına kaydet
            await _context.SaveChangesAsync();
            return Ok(new
            {
                success = true,
                message = "Ürün sepete eklendi",
                totalItems = sepet.SepettekiEtkinlikler.Sum(x => x.Adet)
            });
        }

        // Kullanıcının sepetini getir
        [HttpGet("{kullaniciId}")]
        public IActionResult SepetiGetir(int kullaniciId)
        {
            var sepet = _context.Sepetler
                .Include(s => s.SepettekiEtkinlikler)
                .ThenInclude(se => se.Etkinlik)
                .FirstOrDefault(s => s.KullaniciId == kullaniciId);

            if (sepet == null)
            {
                // Boş sepet oluştur
                sepet = new Cart { KullaniciId = kullaniciId };
                _context.Sepetler.Add(sepet);
                _context.SaveChanges();
            }

            return Ok(new
            {
                SepetId = sepet.Id,
                Items = sepet.SepettekiEtkinlikler.Select(item => new {
                    item.Id,
                    item.EtkinlikId,
                    item.Adet,
                    item.ToplamFiyat,
                    Etkinlik = new
                    {
                        item.Etkinlik.Id,
                        item.Etkinlik.Ad,
                        item.Etkinlik.BiletFiyati
                    }
                })
            });
        }

        /*
        [HttpDelete("sil")]
        public IActionResult SepettenSil(int kullaniciId, int etkinlikId)
        {
            var sepet = _context.Sepetler
                .Include(s => s.SepettekiEtkinlikler)
                .FirstOrDefault(s => s.KullaniciId == kullaniciId);

            if (sepet == null)
                return NotFound("Kullanıcının sepeti bulunamadı.");

            var cartEvent = sepet.SepettekiEtkinlikler
                .FirstOrDefault(ce => ce.EtkinlikId == etkinlikId);

            if (cartEvent == null)
                return NotFound("Etkinlik sepetinizde bulunamadı.");

            _context.SepetEtkinlikler.Remove(cartEvent);
            _context.SaveChanges();

            return Ok("Etkinlik sepetinizden silindi.");
        }

        */

        // Sepetteki ürünler için ödeme yap
        [HttpPost("odeme/{kullaniciId}")]
        public async Task<IActionResult> SepetiOde(int kullaniciId, [FromQuery] string odemeYontemi)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var sepet = await _context.Sepetler
                    .Include(s => s.SepettekiEtkinlikler)
                    .ThenInclude(se => se.Etkinlik)
                    .FirstOrDefaultAsync(s => s.KullaniciId == kullaniciId);

                if (sepet == null || !sepet.SepettekiEtkinlikler.Any())
                    return BadRequest("Sepet boş");

                // Kapasite kontrolü
                foreach (var item in sepet.SepettekiEtkinlikler)
                {
                    if (item.Etkinlik.KalanKapasite < item.Adet)
                    {
                        return BadRequest($"{item.Etkinlik.Ad} için yeterli kapasite yok");
                    }
                }

                // Bilet oluştur
                var biletler = new List<Ticket>();
                foreach (var item in sepet.SepettekiEtkinlikler)
                {
                    for (int i = 0; i < item.Adet; i++)
                    {
                        biletler.Add(new Ticket
                        {
                            KullaniciId = kullaniciId,
                            EtkinlikId = item.EtkinlikId,
                            Fiyat = (int)item.Etkinlik.BiletFiyati,
                            AlimTarihi = DateTime.Now,
                            OdendiMi = true,
                            OdemeYontemi = odemeYontemi
                        });
                    }

                    // Kapasite güncelle
                    item.Etkinlik.KalanKapasite -= item.Adet;
                }

                await _context.Biletler.AddRangeAsync(biletler);
                _context.SepetEtkinlikler.RemoveRange(sepet.SepettekiEtkinlikler);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = "Ödeme başarılı",
                    biletSayisi = biletler.Count
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Ödeme hatası: {ex.Message}"
                });
            }
        }
    }
}

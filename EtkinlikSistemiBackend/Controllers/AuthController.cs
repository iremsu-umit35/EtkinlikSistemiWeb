using EtkinlikSistemiAPI.Data;         // Veritabanı bağlantısı için
using EtkinlikSistemiAPI.Models;       // User modeline erişim
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EtkinlikSistemiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Kullanıcı işlemleri için controller
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Yapıcı metod, veritabanı bağlantısını kurar
        public AuthController(AppDbContext context)
        {
            _context = context;
        }



        // Kayıt (register) işlemi///////////////////////////////
        [HttpPost("register")]
        public IActionResult Register(User yeniKullanici)
        {
            // Aynı e-posta adresiyle daha önce kayıt olunmuş mu?
            var mevcut = _context.Kullanicilar.FirstOrDefault(x => x.Eposta == yeniKullanici.Eposta);
            if (mevcut != null)
            {
                return BadRequest("Bu e-posta ile zaten kayıt olunmuş.");
            }

            // Yeni kullanıcıya varsayılan değerler atanır
            yeniKullanici.Rol = "kullanici";
            yeniKullanici.Onaylandi = false;
            yeniKullanici.SifreDegistirilmeli = true;

            // Veritabanına eklenir
            _context.Kullanicilar.Add(yeniKullanici);
            _context.SaveChanges();

            return Ok("Kayıt başarılı! Yönetici onayını bekleyiniz.");
        }


        // Giriş (login) işlemi/////////////////////////////
        [HttpPost("login")]
        public IActionResult Login([FromBody] User giris)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Bu satır sayesinde hatayı tam görebileceğim
            }
            // E-posta ve şifre ile eşleşen kullanıcıyı bul
            var kullanici = _context.Kullanicilar
                .FirstOrDefault(x => x.Eposta == giris.Eposta && x.Sifre == giris.Sifre);
            // Kullanıcı bulunamazsa yetkisiz dön
            if (kullanici == null)
                return Unauthorized(new { Message = "E-posta veya şifre hatalı" });
            // Hesap onaylı değilse yetkisiz dön
            if (!kullanici.Onaylandi)
                return Unauthorized(new { Message = "Hesabınız henüz onaylanmamış" });
            // Şifre değişikliği gerekiyorsa kullanıcıyı bilgilendir
            if (kullanici.SifreDegistirilmeli)
            {
                return Ok(new
                {
                    Message = "Şifrenizi değiştirin",
                    RequiresPasswordChange = true,
                    Rol = kullanici.Rol,
                    UserId = kullanici.Id
                });
            }

            // Basit bir token simülasyonu (gerçek projede JWT kullanın)
            var token = Guid.NewGuid().ToString();
            // Giriş başarılı yanıtı
            return Ok(new
            {
                Token = token,
                Message = "Giriş başarılı",
                Rol = kullanici.Rol,
                UserId = kullanici.Id
            });
        }



        // Şifre değiştirme işlemi (ilk girişte zorunlu olabilir)//////////////////////
        [HttpPost("change-password")]
        public IActionResult ChangePassword([FromBody] PasswordChangeRequest request)
        {
            // Eski şifre ile kullanıcıyı bul
            var kullanici = _context.Kullanicilar
                .FirstOrDefault(x => x.Eposta == request.Eposta && x.Sifre == request.EskiSifre);

            if (kullanici == null)
                return Unauthorized(new { Message = "Eski şifre hatalı." });
            // Yeni şifreyi ata ve flag'i kaldır
            kullanici.Sifre = request.YeniSifre;
            kullanici.SifreDegistirilmeli = false;
            _context.SaveChanges();

            return Ok(new { Message = "Şifre başarıyla değiştirildi." });
        }

        // Giriş yapan kullanıcıyı ID'sine göre getir//////////////////////////////////
        [Authorize]
        [HttpGet("user")]
        public IActionResult GetCurrentUser()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);
            var user = _context.Kullanicilar.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.Eposta,
                user.Rol,
                user.Onaylandi,
                user.SifreDegistirilmeli
            });
        }


    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EtkinlikSistemiAPI.Models;
using Microsoft.EntityFrameworkCore;
using EtkinlikSistemiAPI.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EtkinlikSistemiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Yapıcı metod, veritabanı bağlantısını kurar
        public EventController(AppDbContext context)
        {
            _context = context;
        }

        //  1. Tüm etkinlikleri listele
        // Tüm etkinlikleri tarihe göre sıralı getir
        [HttpGet("tum")]
        public IActionResult TumEtkinlikleriGetir()
        {
            var etkinlikler = _context.Etkinlikler.OrderBy(e => e.Tarih).ToList();
            return Ok(etkinlikler);
        }
        
    }
}

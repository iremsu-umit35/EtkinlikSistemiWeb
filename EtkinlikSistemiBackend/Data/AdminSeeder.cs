using EtkinlikSistemiAPI.Models;
using EtkinlikSistemiAPI.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EtkinlikSistemiAPI
{
    public static class AdminSeeder
    {
        // Admin kullanıcısını sisteme eklemek için kullanılan metod
        public static void SeedAdmin(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Admin daha önce eklenmiş mi kontrol et
            if (!context.Kullanicilar.Any(u => u.Rol == "admin"))
            {
                var admin = new User
                {
                    Eposta = "admin@example.com",
                    Sifre = "Admin123!", // Burada şifreyi hash'lemen gerekebilir
                    Rol = "admin",
                    Onaylandi = true,
                    SifreDegistirilmeli = false
                };

                context.Kullanicilar.Add(admin);
                context.SaveChanges(); // Veritabanına kaydet
                Console.WriteLine("Admin kullanıcısı başarıyla eklendi!"); 
            }
            else
            {
                Console.WriteLine("Admin kullanıcısı zaten mevcut."); 
            }
        }

    }
}
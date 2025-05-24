using Microsoft.EntityFrameworkCore;
using EtkinlikSistemiAPI.Models; // User sınıfı buradan geliyor
namespace EtkinlikSistemiAPI.Data
{
    public class AppDbContext: DbContext
    {
        
        // Veritabanında oluşacak tabloyu temsil eder.
        // User sınıfına ait kayıtlar "Kullanicilar" tablosunda tutulacak.
        public DbSet<User> Kullanicilar { get; set; }

        // event sınıfına ait kayıtlar "etkinlikler" tablosunda tutulacak.
        public DbSet<Event> Etkinlikler { get; set; } //etkinlik için
        public DbSet<Ticket> Biletler { get; set; } //bilet için
        public DbSet<Cart> Sepetler { get; set; } //sepet için
        public DbSet<CartEvent> SepetEtkinlikler { get; set; } //sepetteki etkinlik için
        public DbSet<announcement> Announcements { get; set; } //duyuru için
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // BiletFiyati için hassasiyet ayarı
            modelBuilder.Entity<Event>()
                .Property(e => e.BiletFiyati)
                .HasPrecision(18, 2);
        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
                
    }
}

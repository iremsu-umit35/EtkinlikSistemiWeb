using Microsoft.AspNetCore.DataProtection;

namespace EtkinlikSistemiAPI.Models
{
    //Bu model, sepetin içinde hangi etkinliklerin olduğunu, her etkinlikten kaç tane olduğunu ve toplam fiyat bilgisini tutar.
    public class CartEvent
    {
        public int Id { get; set; }                    // Sepet etkinlik ID'si
        public int SepetId { get; set; }               // Sepetin ID'si
        public int EtkinlikId { get; set; }            // Sepetteki etkinliğin ID'si
        public int Adet { get; set; }                  // Sepetteki etkinlik adedi
        public int ToplamFiyat { get; set; }           // Etkinliğin toplam fiyatı (adet * fiyat)
        public virtual Cart Sepet { get; set; }       // Sepeti temsil eder
        public virtual Event Etkinlik { get; set; } // Sepetteki etkinliği temsil eder
    }
}

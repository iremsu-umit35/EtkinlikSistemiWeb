namespace EtkinlikSistemiAPI.Models
{
    public class Event
    {
        public int Id { get; set; }                  // Etkinlik kimliği
        public string Ad { get; set; }               // Etkinlik adı
        public string Aciklama { get; set; }         // Etkinlik açıklaması
        public string Tur { get; set; }              // Etkinlik türü (ör: Konser, Tiyatro)
        public DateTime Tarih { get; set; }          // Etkinlik tarihi
        public int Kapasite { get; set; }            // Maksimum katılımcı sayısı
        public int KalanKapasite { get; set; }       // Kalan bilet sayısı
        public string Lokasyon { get; set; }         // Etkinlik yeri (şehir, açık/kapalı)
        public decimal BiletFiyati { get; set; }     // 💸 Bilet fiyatı eklendi
       
        public string HavaDurumu { get; set; } // Hava durumu bilgisi (API'dan çekilecek)
        public bool PlanlanabilirMi { get; set; } // true/false
    }
}

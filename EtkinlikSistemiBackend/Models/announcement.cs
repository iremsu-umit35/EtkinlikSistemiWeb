namespace EtkinlikSistemiAPI.Models
{
    public class announcement
    {
        public int Id { get; set; }
        public string Baslik { get; set; }
        public string Icerik { get; set; }
        public DateTime Tarih { get; set; }
        public bool IsPublished { get; set; } // Yayın durumu
    }
}

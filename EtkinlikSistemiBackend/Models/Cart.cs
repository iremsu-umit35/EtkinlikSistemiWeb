namespace EtkinlikSistemiAPI.Models
{
    //Sepet modeli, kullanıcının sepetinde hangi etkinliklerin olduğunu ve bu etkinliklerin ne kadar olduğunu takip eder.
    public class Cart
    {
        public int Id { get; set; }                // Sepetin ID'si
        public int KullaniciId { get; set; }       // Sepeti oluşturan kullanıcının ID'si
        public virtual ICollection<CartEvent> SepettekiEtkinlikler { get; set; } = new List<CartEvent>();  // Sepetteki etkinlikleri tutar
        public virtual User Kullanici { get; set; }  // Sepetin sahibi kullanıcı
    }
}

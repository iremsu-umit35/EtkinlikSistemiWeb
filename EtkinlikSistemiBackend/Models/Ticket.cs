namespace EtkinlikSistemiAPI.Models
{

    //Bu model, biletin hangi kullanıcıya ait olduğunu ve hangi etkinlik için alındığını takip edebilmenizi sağlar.


    public class Ticket
    {
        public int Id { get; set; }                  // Biletin ID'si
        public int KullaniciId { get; set; }         // Bileti satın alan kullanıcının ID'si
        public int EtkinlikId { get; set; }          // Hangi etkinlik için bilet alındığı
        public int Fiyat { get; set; }               // Biletin fiyatı
        public DateTime AlimTarihi { get; set; }     // Biletin alındığı tarih
        public bool OdendiMi { get; set; }           // Biletin ödenip ödenmediği bilgisi

        public virtual User Kullanici { get; set; }  // Biletin sahibini referans alır
        public virtual Event Etkinlik { get; set; }    // Biletin hangi etkinlik için olduğunu belirtir
        public string OdemeYontemi { get; set; }  // Ödeme yöntemi: "Kredi Kartı", "Havale", vs.
    }
}


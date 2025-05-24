namespace EtkinlikSistemiAPI.Models
{
    public class User
    {
        public int Id { get; set; }                       // Kullanıcı kimliği
        public string Eposta { get; set; }                // E-posta adresi
        public string Sifre { get; set; }                 // Şifre
        public string Rol { get; set; } = "kullanici";  // Varsayılan değer verilebilir
        public bool Onaylandi { get; set; } = false;      // Yönetici onayı
        public bool SifreDegistirilmeli { get; set; } = true; // İlk girişte şifre değiştirilsin mi?
        public string? Token { get; set; }

    }
}

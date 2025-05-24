namespace EtkinlikSistemiAPI.Models
{
    public class PasswordChangeRequest
    {
        public string Eposta { get; set; }
        public string EskiSifre { get; set; }
        public string YeniSifre { get; set; }
    }
}

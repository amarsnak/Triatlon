// Models/Tekmovanje.cs
namespace Triatlon.Models
{
    public class Tekmovanje
    {
        public int Id { get; set; }
        public string? Naziv { get; set; }
        public string? Lokacija { get; set; }
        public string? Tip { get; set; }
        public DateTime? Datum { get; set; }
        public int? UporabnikId { get; set; }
    }
}

// Models/Kategorija.cs
namespace Triatlon.Models
{
    public class Kategorija
    {
        public int Id { get; set; }
        public string? Naziv { get; set; }
        public string? Spol { get; set; }
        public int? MinStarost { get; set; }
        public int? MaxStarost { get; set; }
    }
}

// Models/Uporabnik.cs
namespace Triatlon.Models
{
    public class Uporabnik
    {
        public int Id { get; set; }
        public string? UporabniskoIme { get; set; }
        public string? GesloHash { get; set; }
        public string? Vloga { get; set; }
        public string? Email { get; set; }
    }
}

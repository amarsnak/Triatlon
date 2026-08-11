// Models/Rezultat.cs
namespace Triatlon.Models
{
    public class Rezultat
    {
        public int Id { get; set; }
        public int TekovanjeId { get; set; }
        public int TekmovalecId { get; set; }
        public int? KategorijaId { get; set; }
        public string? Bib { get; set; }
        public int? UvrstevSkupna { get; set; }
        public int? UvrstevSpol { get; set; }
        public int? UvrstevKategorija { get; set; }
        public TimeSpan? CasPlavanje { get; set; }
        public TimeSpan? CasT1 { get; set; }
        public TimeSpan? CasKolesarjenje { get; set; }
        public TimeSpan? CasT2 { get; set; }
        public TimeSpan? CasTek { get; set; }
        public TimeSpan? CasSkupni { get; set; }
        public double? Tocke { get; set; }
    }
}

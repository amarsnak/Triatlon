// Models/Modeli.cs
using System;
using System.Text.Json.Serialization;

namespace Naloga4_Splet.Models
{
    public class Tekmovalec
    {
        [JsonPropertyName("id")]      public int Id { get; set; }
        [JsonPropertyName("ime")]     public string? Ime { get; set; }
        [JsonPropertyName("priimek")] public string? Priimek { get; set; }
        [JsonPropertyName("starost")] public int? Starost { get; set; }
        [JsonPropertyName("kraj")]    public string? Kraj { get; set; }
        [JsonPropertyName("drzava")]  public string? Drzava { get; set; }
        [JsonPropertyName("poklic")]  public string? Poklic { get; set; }

        public string ImeInPriimek => $"{Ime} {Priimek}";
    }

    public class Tekmovanje
    {
        [JsonPropertyName("id")]          public int Id { get; set; }
        [JsonPropertyName("naziv")]       public string? Naziv { get; set; }
        [JsonPropertyName("lokacija")]    public string? Lokacija { get; set; }
        [JsonPropertyName("datum")]       public DateTime? Datum { get; set; }
        [JsonPropertyName("tip")]         public string? Tip { get; set; }
        [JsonPropertyName("uporabnikId")] public int? UporabnikId { get; set; }
    }

    public class Rezultat
    {
        [JsonPropertyName("id")]                public int Id { get; set; }
        [JsonPropertyName("tekovanjeId")]       public int TekovanjeId { get; set; }
        [JsonPropertyName("tekmovalecId")]      public int TekmovalecId { get; set; }
        [JsonPropertyName("kategorijaId")]      public int? KategorijaId { get; set; }
        [JsonPropertyName("bib")]               public string? Bib { get; set; }
        [JsonPropertyName("uvrstevSkupna")]     public int? UvrstevSkupna { get; set; }
        [JsonPropertyName("uvrstevSpol")]       public int? UvrstevSpol { get; set; }
        [JsonPropertyName("uvrstevKategorija")] public int? UvrstevKategorija { get; set; }
        [JsonPropertyName("casPlavanja")]       public TimeSpan? CasPlavanje { get; set; }
        [JsonPropertyName("casT1")]             public TimeSpan? CasT1 { get; set; }
        [JsonPropertyName("casKolesarjenje")]   public TimeSpan? CasKolesarjenje { get; set; }
        [JsonPropertyName("casT2")]             public TimeSpan? CasT2 { get; set; }
        [JsonPropertyName("casTek")]            public TimeSpan? CasTek { get; set; }
        [JsonPropertyName("casSkupni")]         public TimeSpan? CasSkupni { get; set; }
        [JsonPropertyName("tocke")]             public double? Tocke { get; set; }

        public string CasFinish => CasSkupni?.ToString(@"hh\:mm\:ss") ?? CasPlavanje?.ToString(@"hh\:mm\:ss") ?? "—";
    }

    public class LestvicaVrstica
    {
        public int? Uvrstitev { get; set; }
        public string? Ime { get; set; }
        public string? Priimek { get; set; }
        public string? Drzava { get; set; }
        public string? CasPlavanje { get; set; }
        public string? CasKolesarjenje { get; set; }
        public string? CasTek { get; set; }
        public string? CasSkupni { get; set; }
        public double? Tocke { get; set; }
    }

    public class StatistikaTekmovalca
    {
        public int TekmovalecId { get; set; }
        public long SteviloNastopov { get; set; }
        public string? NajboljsiCas { get; set; }
        public string? PovprecniCas { get; set; }
        public string? NajboljsiPlavanje { get; set; }
        public string? NajboljsiKolesarjenje { get; set; }
        public string? NajboljsiTek { get; set; }
    }

    public class Kategorija
    {
        [JsonPropertyName("id")]         public int Id { get; set; }
        [JsonPropertyName("naziv")]      public string? Naziv { get; set; }
        [JsonPropertyName("spol")]       public string? Spol { get; set; }
        [JsonPropertyName("minStarost")] public int? MinStarost { get; set; }
        [JsonPropertyName("maxStarost")] public int? MaxStarost { get; set; }
    }

    public class Uporabnik
    {
        public int Id { get; set; }
        public string? UporabniskoIme { get; set; }
        public string? GesloHash { get; set; }
        public string? Email { get; set; }
        public string? Vloga { get; set; }
    }

    public class LoginModel
    {
        public string? UporabniskoIme { get; set; }
        public string? Geslo { get; set; }
    }

    public class RegisterModel
    {
        public string? UporabniskoIme { get; set; }
        public string? Email { get; set; }
        public string? Geslo { get; set; }
    }
}

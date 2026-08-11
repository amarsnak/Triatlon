// Models/Modeli.cs
using System;
using System.Text.Json.Serialization;

namespace Naloga3_WPF.Models
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
        [JsonPropertyName("id")]                  public int Id { get; set; }
        [JsonPropertyName("tekovanjeId")]         public int TekovanjeId { get; set; }
        [JsonPropertyName("tekmovalecId")]        public int TekmovalecId { get; set; }
        [JsonPropertyName("kategorijaId")]        public int? KategorijaId { get; set; }
        [JsonPropertyName("bib")]                 public string? Bib { get; set; }
        [JsonPropertyName("uvrstevSkupna")]       public int? UvrstevSkupna { get; set; }
        [JsonPropertyName("uvrstevSpol")]         public int? UvrstevSpol { get; set; }
        [JsonPropertyName("uvrstevKategorija")]   public int? UvrstevKategorija { get; set; }
        [JsonPropertyName("casPlavanja")]         public TimeSpan? CasPlavanje { get; set; }
        [JsonPropertyName("casT1")]               public TimeSpan? CasT1 { get; set; }
        [JsonPropertyName("casKolesarjenje")]     public TimeSpan? CasKolesarjenje { get; set; }
        [JsonPropertyName("casT2")]               public TimeSpan? CasT2 { get; set; }
        [JsonPropertyName("casTek")]              public TimeSpan? CasTek { get; set; }
        [JsonPropertyName("casSkupni")]           public TimeSpan? CasSkupni { get; set; }
        [JsonPropertyName("tocke")]               public double? Tocke { get; set; }

        public string Tekmovalec => $"ID: {TekmovalecId}";
        public string Tekmovanje => $"ID: {TekovanjeId}";
        public string CasFinish  => CasSkupni?.ToString(@"hh\:mm\:ss") ?? CasPlavanje?.ToString(@"hh\:mm\:ss") ?? "";
        public int?   Mesto      => UvrstevSkupna;
    }

    public class Uporabnik
    {
        [JsonPropertyName("id")]              public int Id { get; set; }
        [JsonPropertyName("uporabniskoIme")]  public string? UporabniskoIme { get; set; }
        [JsonPropertyName("gesloHash")]       public string? GesloHash { get; set; }
        [JsonPropertyName("vloga")]           public string? Vloga { get; set; }
        [JsonPropertyName("email")]           public string? Email { get; set; }
    }

    public class StatistikaRow
    {
        public string? Tip { get; set; }
        public int Tekmovanj { get; set; }
        public int Rezultatov { get; set; }
    }
}

// Pages/Filter.cshtml.cs
using Microsoft.AspNetCore.Mvc.RazorPages;
using Naloga4_Splet.Services;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Naloga4_Splet.Pages
{
    public class FilterRezultat
    {
        public string? Ime { get; set; }
        public string? Priimek { get; set; }
        public string? Drzava { get; set; }
        public int? Starost { get; set; }
        public string? Kategorija { get; set; }
        public int? Uvrstitev { get; set; }
        public string? CasSkupni { get; set; }
    }

    public class FilterModel : PageModel
    {
        private readonly ApiService _api;
        public List<FilterRezultat> Rezultati { get; set; } = new();
        public string? Drzava { get; set; }
        public string? Kategorija { get; set; }
        public int? MinStarost { get; set; }
        public int? MaxStarost { get; set; }
        public bool IsFiltered { get; set; }

        public FilterModel(ApiService api) => _api = api;

        public async Task OnGetAsync(string? drzava, string? kategorija, int? minStarost, int? maxStarost)
        {
            Drzava     = drzava;
            Kategorija = kategorija;
            MinStarost = minStarost;
            MaxStarost = maxStarost;

            if (string.IsNullOrEmpty(drzava) && string.IsNullOrEmpty(kategorija)
                && !minStarost.HasValue && !maxStarost.HasValue)
                return;

            IsFiltered = true;
            try
            {
                var json = await _api.GetFilterJsonAsync(drzava, kategorija, minStarost, maxStarost);
                if (string.IsNullOrEmpty(json)) return;

                using var doc = JsonDocument.Parse(json);
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    Rezultati.Add(new FilterRezultat
                    {
                        Ime        = element.TryGetProperty("ime",        out var v1) ? v1.GetString() : null,
                        Priimek    = element.TryGetProperty("priimek",    out var v2) ? v2.GetString() : null,
                        Drzava     = element.TryGetProperty("drzava",     out var v3) ? v3.GetString() : null,
                        Starost    = element.TryGetProperty("starost",    out var v4) && v4.ValueKind != JsonValueKind.Null ? v4.GetInt32() : null,
                        Kategorija = element.TryGetProperty("kategorija", out var v5) ? v5.GetString() : null,
                        Uvrstitev  = element.TryGetProperty("uvrstitev",  out var v6) && v6.ValueKind != JsonValueKind.Null ? v6.GetInt32() : null,
                        CasSkupni  = element.TryGetProperty("casSkupni",  out var v7) ? v7.GetString() : null,
                    });
                }
            }
            catch { }
        }
    }
}

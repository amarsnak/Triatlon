// Pages/Tekmovalec.cshtml.cs
using Microsoft.AspNetCore.Mvc.RazorPages;
using Naloga4_Splet.Models;
using Naloga4_Splet.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Naloga4_Splet.Pages
{
    public class TekmovalecModel : PageModel
    {
        private readonly ApiService _api;
        public Tekmovalec? Tekmovalec { get; set; }
        public List<Rezultat> Rezultati { get; set; } = new();
        public StatistikaTekmovalca? Statistika { get; set; }
        public string GrafCasovi { get; set; } = "[]";
        public string GrafLabels { get; set; } = "[]";

        public TekmovalecModel(ApiService api) => _api = api;

        public async Task OnGetAsync(int id)
        {
            Tekmovalec = await _api.GetTekmovalecAsync(id);
            if (Tekmovalec == null) return;

            Rezultati  = await _api.GetRezultatiTekmovalcaAsync(id);
            Statistika = await _api.GetStatistikaTekmovalcaAsync(id);

            // Graf napredka — skupni čas v minutah
            var zCasom = Rezultati
                .Where(r => r.CasSkupni.HasValue)
                .OrderBy(r => r.TekovanjeId)
                .ToList();

            GrafCasovi = JsonSerializer.Serialize(
                zCasom.Select(r => Math.Round(r.CasSkupni!.Value.TotalMinutes, 1)).ToList());
            GrafLabels = JsonSerializer.Serialize(
                zCasom.Select((r, i) => $"#{i + 1} (ID:{r.TekovanjeId})").ToList());
        }
    }
}

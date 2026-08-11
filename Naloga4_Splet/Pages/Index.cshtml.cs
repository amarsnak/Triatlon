// Pages/Index.cshtml.cs
using Microsoft.AspNetCore.Mvc.RazorPages;
using Naloga4_Splet.Models;
using Naloga4_Splet.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Naloga4_Splet.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApiService _api;

        public int SteviloTekmovalcev { get; set; }
        public int SteviloTekmovanj { get; set; }
        public int SteviloRezultatov { get; set; }
        public List<Tekmovanje> ZadnjaTekmovanja { get; set; } = new();
        public string TipLabels { get; set; } = "[]";
        public string TipData { get; set; } = "[]";

        public IndexModel(ApiService api) => _api = api;

        public async Task OnGetAsync()
        {
            try
            {
                var tekmovalci = await _api.GetTekmovalciAsync(limit: 1);
                var tekmovanja = await _api.GetTekmovanjaAsync();

                SteviloTekmovalcev = 2137266; // iz baze
                SteviloTekmovanj   = tekmovanja.Count;
                SteviloRezultatov  = 599451;  // iz baze

                ZadnjaTekmovanja = tekmovanja.Take(8).ToList();

                // Graf po tipu
                var porTip = tekmovanja
                    .GroupBy(t => t.Tip ?? "Neznano")
                    .OrderByDescending(g => g.Count())
                    .ToList();

                TipLabels = JsonSerializer.Serialize(porTip.Select(g => g.Key).ToList());
                TipData   = JsonSerializer.Serialize(porTip.Select(g => g.Count()).ToList());
            }
            catch { }
        }
    }
}

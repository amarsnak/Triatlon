// Pages/Tekmovanja.cshtml.cs
using Microsoft.AspNetCore.Mvc.RazorPages;
using Naloga4_Splet.Models;
using Naloga4_Splet.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Naloga4_Splet.Pages
{
    public class TekmovanjaModel : PageModel
    {
        private readonly ApiService _api;
        public List<Tekmovanje> Tekmovanja { get; set; } = new();
        public string? Lokacija { get; set; }
        public string? Tip { get; set; }

        public TekmovanjaModel(ApiService api) => _api = api;

        public async Task OnGetAsync(string? lokacija, string? tip)
        {
            Lokacija = lokacija;
            Tip      = tip;

            var vse = await _api.GetTekmovanjaAsync();

            if (!string.IsNullOrWhiteSpace(lokacija))
                vse = vse.Where(t => (t.Lokacija ?? "").ToLower().Contains(lokacija.ToLower())).ToList();

            if (!string.IsNullOrWhiteSpace(tip))
                vse = vse.Where(t => t.Tip == tip).ToList();

            Tekmovanja = vse.OrderByDescending(t => t.Datum).ToList();
        }
    }
}

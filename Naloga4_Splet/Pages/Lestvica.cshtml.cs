// Pages/Lestvica.cshtml.cs
using Microsoft.AspNetCore.Mvc.RazorPages;
using Naloga4_Splet.Models;
using Naloga4_Splet.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Naloga4_Splet.Pages
{
    public class LestvicaModel : PageModel
    {
        private readonly ApiService _api;
        public List<LestvicaVrstica> Lestvica { get; set; } = new();
        public List<Tekmovanje> VsaTekmovanja { get; set; } = new();
        public Tekmovanje? IzbranTekmovanje { get; set; }
        public int? TekmovanjeId { get; set; }

        public LestvicaModel(ApiService api) => _api = api;

        public async Task OnGetAsync(int? tekmovanjeId)
        {
            TekmovanjeId    = tekmovanjeId;
            VsaTekmovanja   = await _api.GetTekmovanjaAsync();

            if (tekmovanjeId.HasValue)
            {
                Lestvica          = await _api.GetLestvicaAsync(tekmovanjeId.Value);
                IzbranTekmovanje  = await _api.GetTekmovanjeAsync(tekmovanjeId.Value);
            }
        }
    }
}

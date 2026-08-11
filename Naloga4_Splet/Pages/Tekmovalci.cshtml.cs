// Pages/Tekmovalci.cshtml.cs
using Microsoft.AspNetCore.Mvc.RazorPages;
using Naloga4_Splet.Models;
using Naloga4_Splet.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Naloga4_Splet.Pages
{
    public class TekmovalciModel : PageModel
    {
        private readonly ApiService _api;
        public List<Tekmovalec> Tekmovalci { get; set; } = new();
        public string? Iskanje { get; set; }

        public TekmovalciModel(ApiService api) => _api = api;

        public async Task OnGetAsync(string? iskanje)
        {
            Iskanje = iskanje;
            if (!string.IsNullOrWhiteSpace(iskanje))
                Tekmovalci = await _api.IscitekmovalceAsync(iskanje);
        }
    }
}

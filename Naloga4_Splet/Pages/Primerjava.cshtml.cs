// Pages/Primerjava.cshtml.cs
using Microsoft.AspNetCore.Mvc.RazorPages;
using Naloga4_Splet.Models;
using Naloga4_Splet.Services;
using System.Threading.Tasks;

namespace Naloga4_Splet.Pages
{
    public class PrimerjavaModel : PageModel
    {
        private readonly ApiService _api;
        public Tekmovalec? T1 { get; set; }
        public Tekmovalec? T2 { get; set; }
        public StatistikaTekmovalca? Stat1 { get; set; }
        public StatistikaTekmovalca? Stat2 { get; set; }
        public int? Id1 { get; set; }
        public int? Id2 { get; set; }

        public PrimerjavaModel(ApiService api) => _api = api;

        public async Task OnGetAsync(int? id1, int? id2)
        {
            Id1 = id1;
            Id2 = id2;

            if (id1.HasValue && id2.HasValue)
            {
                T1    = await _api.GetTekmovalecAsync(id1.Value);
                T2    = await _api.GetTekmovalecAsync(id2.Value);
                Stat1 = await _api.GetStatistikaTekmovalcaAsync(id1.Value);
                Stat2 = await _api.GetStatistikaTekmovalcaAsync(id2.Value);
            }
        }
    }
}

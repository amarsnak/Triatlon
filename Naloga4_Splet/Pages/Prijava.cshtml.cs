// Pages/Prijava.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Naloga4_Splet.Services;
using System.Threading.Tasks;

namespace Naloga4_Splet.Pages
{
    public class PrijavaModel : PageModel
    {
        private readonly ApiService _api;
        public string? Napaka { get; set; }

        public PrijavaModel(ApiService api) => _api = api;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync(string uporabniskoIme, string geslo)
        {
            if (string.IsNullOrWhiteSpace(uporabniskoIme) || string.IsNullOrWhiteSpace(geslo))
            {
                Napaka = "Vnesi uporabniško ime in geslo.";
                return Page();
            }

            // Pridobi vse uporabnike in preveri
            var uporabniki = await _api.GetUporabnikiAsync();
            var u = uporabniki.Find(x =>
                x.UporabniskoIme == uporabniskoIme &&
                x.GesloHash == geslo);

            if (u != null)
            {
                HttpContext.Session.SetString("UporabnikIme",  u.UporabniskoIme ?? uporabniskoIme);
                HttpContext.Session.SetString("UporabnikVloga", u.Vloga ?? "uporabnik");
                return RedirectToPage("/Index");
            }

            Napaka = "Napačno uporabniško ime ali geslo.";
            return Page();
        }
    }
}

// Pages/Registracija.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Naloga4_Splet.Services;
using System.Threading.Tasks;

namespace Naloga4_Splet.Pages
{
    public class RegistracijaModel : PageModel
    {
        private readonly ApiService _api;
        public string? Napaka { get; set; }
        public bool Uspesno { get; set; }

        public RegistracijaModel(ApiService api) => _api = api;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync(string uporabniskoIme, string email, string geslo)
        {
            if (string.IsNullOrWhiteSpace(uporabniskoIme) || string.IsNullOrWhiteSpace(geslo))
            {
                Napaka = "Vsa polja so obvezna.";
                return Page();
            }

            var ok = await _api.RegisterAsync(uporabniskoIme, email, geslo);
            if (ok)
            {
                Uspesno = true;
                return Page();
            }

            Napaka = "Registracija ni uspela. Poskusite znova.";
            return Page();
        }
    }
}

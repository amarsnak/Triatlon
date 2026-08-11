// Services/JezikService.cs
namespace Naloga3_WPF.Services
{
    public enum Jezik { SLO, ENG }

    public class JezikService
    {
        public Jezik Trenutni { get; private set; } = Jezik.SLO;

        public void Preklopi() =>
            Trenutni = Trenutni == Jezik.SLO ? Jezik.ENG : Jezik.SLO;

        public string T(string slo, string eng) =>
            Trenutni == Jezik.SLO ? slo : eng;
    }
}

// ViewModels/MainViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Naloga3_WPF.Models;
using Naloga3_WPF.Services;

namespace Naloga3_WPF.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        { _execute = execute; _canExecute = canExecute; }
        public bool CanExecute(object? p) => _canExecute?.Invoke() ?? true;
        public void Execute(object? p) => _execute();
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _api = new();
        private readonly JezikService _jez = new();

        public static MainWindow? MainWindowRef { get; set; }

        private System.Collections.Generic.List<Tekmovalec> _vsiTekmovalci = new();

        public ObservableCollection<Tekmovalec> Tekmovalci { get; } = new();
        public ObservableCollection<Tekmovanje> Tekmovanja { get; } = new();
        public ObservableCollection<Rezultat> Rezultati { get; } = new();
        public ObservableCollection<Uporabnik> Uporabniki { get; } = new();
        public ObservableCollection<StatistikaRow> StatPoTipih { get; } = new();
        public ObservableCollection<string> FilterDrzave { get; } = new();
        public ObservableCollection<string> VlogaOpcije { get; } = new() { "admin", "uporabnik" };

        private string _iskalniNiz = "";
        public string IskalniNiz
        {
            get => _iskalniNiz;
            set { _iskalniNiz = value; OnPropertyChanged(); PrimeniFilter(); }
        }

        private string? _izbranaDrzava;
        public string? IzbranaDrzava
        {
            get => _izbranaDrzava;
            set { _izbranaDrzava = value; OnPropertyChanged(); PrimeniFilter(); }
        }

        private void PrimeniFilter()
        {
            var q = _vsiTekmovalci.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(_iskalniNiz))
            {
                var niz = _iskalniNiz.ToLower();
                q = q.Where(t =>
                    (t.Ime ?? "").ToLower().Contains(niz) ||
                    (t.Priimek ?? "").ToLower().Contains(niz));
            }
            if (!string.IsNullOrWhiteSpace(_izbranaDrzava) && _izbranaDrzava != "—")
                q = q.Where(t => t.Drzava == _izbranaDrzava);
            Tekmovalci.Clear();
            foreach (var t in q) Tekmovalci.Add(t);
            OnPropertyChanged(nameof(RezultatiStevilka));
        }

        private Tekmovalec? _izbranT;
        public Tekmovalec? IzbranTekmovalec
        { get => _izbranT; set { _izbranT = value; OnPropertyChanged(); } }

        private Tekmovanje? _izbranTm;
        public Tekmovanje? IzbranTekmovanje
        { get => _izbranTm; set { _izbranTm = value; OnPropertyChanged(); } }

        private Uporabnik? _izbranU;
        public Uporabnik? IzbranUporabnik
        { get => _izbranU; set { _izbranU = value; OnPropertyChanged(); } }

        private string _tIme = "", _tPriimek = "", _tStarost = "", _tKraj = "", _tDrzava = "", _tPoklic = "";
        public string TIme { get => _tIme; set { _tIme = value; OnPropertyChanged(); } }
        public string TPriimek { get => _tPriimek; set { _tPriimek = value; OnPropertyChanged(); } }
        public string TStarost { get => _tStarost; set { _tStarost = value; OnPropertyChanged(); } }
        public string TKraj { get => _tKraj; set { _tKraj = value; OnPropertyChanged(); } }
        public string TDrzava { get => _tDrzava; set { _tDrzava = value; OnPropertyChanged(); } }
        public string TPoklic { get => _tPoklic; set { _tPoklic = value; OnPropertyChanged(); } }

        private string _tNaziv = "", _tLokacija = "", _tDatum = "", _tTip = "";
        public string TNaziv { get => _tNaziv; set { _tNaziv = value; OnPropertyChanged(); } }
        public string TLokacija { get => _tLokacija; set { _tLokacija = value; OnPropertyChanged(); } }
        public string TDatum { get => _tDatum; set { _tDatum = value; OnPropertyChanged(); } }
        public string TTip { get => _tTip; set { _tTip = value; OnPropertyChanged(); } }

        private string _tUporabniskoIme = "", _tEmail = "", _tVloga = "uporabnik";
        public string TUporabniskoIme { get => _tUporabniskoIme; set { _tUporabniskoIme = value; OnPropertyChanged(); } }
        public string TEmail { get => _tEmail; set { _tEmail = value; OnPropertyChanged(); } }
        public string TVloga { get => _tVloga; set { _tVloga = value; OnPropertyChanged(); } }

        private string _status = "Pripravljeno.";
        public string StatusSporocilo { get => _status; set { _status = value; OnPropertyChanged(); } }

        private int _skupajTekmovalcev, _skupajTekmovanj, _skupajRezultatov;
        public string StatSkupajTekmovalcev => _skupajTekmovalcev.ToString();
        public string StatSkupajTekmovanj => _skupajTekmovanj.ToString();
        public string StatSkupajRezultatov => _skupajRezultatov.ToString();
        public string RezultatiStevilka => _jez.T(
            $"Prikazano: {Tekmovalci.Count} / {_skupajTekmovalcev}",
            $"Showing: {Tekmovalci.Count} / {_skupajTekmovalcev}");

        public string WindowTitle => _jez.T("Triatlon — Upravljanje", "Triathlon — Management");
        public string NaslovAplikacije => _jez.T("🏃 Triatlon Upravljanje", "🏃 Triathlon Management");
        public string JezikGumb => _jez.T("🌐 ENG", "🌐 SLO");
        public string TabTekmovalci => _jez.T("Tekmovalci", "Athletes");
        public string TabTekmovanja => _jez.T("Tekmovanja", "Races");
        public string TabRezultati => _jez.T("Rezultati", "Results");
        public string TabStatistika => _jez.T("Statistika", "Statistics");
        public string TabUporabniki => _jez.T("Uporabniki", "Users");
        public string FormNaslovTekmovalec => _jez.T("Podatki tekmovalca", "Athlete Details");
        public string FormNaslovTekmovanje => _jez.T("Podatki tekmovanja", "Race Details");
        public string FormNaslovUporabnik => _jez.T("Podatki uporabnika", "User Details");
        public string GumbShrani => _jez.T("Shrani", "Save");
        public string GumbIzbrisi => _jez.T("Izbriši", "Delete");
        public string GumbNov => _jez.T("Nov", "New");
        public string GumbPocistiFilter => _jez.T("✕ Počisti", "✕ Clear");
        public string GumbIzvozCSV => _jez.T("⬇ Izvozi CSV", "⬇ Export CSV");
        public string IskalniPlaceholder => _jez.T("Išči po imenu ali priimku...", "Search by name...");
        public string StatNaslov => _jez.T("Statistika baze", "Database Statistics");
        public string StatLabelTekmovalcev => _jez.T("Tekmovalcev", "Athletes");
        public string StatLabelTekmovanj => _jez.T("Tekmovanj", "Races");
        public string StatLabelRezultatov => _jez.T("Rezultatov", "Results");
        public string StatPoTipihNaslov => _jez.T("Po tipu tekmovanja", "By race type");

        public ICommand PreklopJezikCmd { get; }
        public ICommand ShraniTekmovalecCmd { get; }
        public ICommand IzbrisiTekmovalecCmd { get; }
        public ICommand NovTekmovalecCmd { get; }
        public ICommand ShraniTekmovanjeCmd { get; }
        public ICommand IzbrisiTekmovanjeCmd { get; }
        public ICommand NovTekmovanjeCmd { get; }
        public ICommand PocistiFilterCmd { get; }
        public ICommand IzvozCSVCmd { get; }
        public ICommand ShraniUporabnikCmd { get; }
        public ICommand IzbrisiUporabnikCmd { get; }
        public ICommand NovUporabnikCmd { get; }

        public MainViewModel()
        {
            PreklopJezikCmd = new RelayCommand(PreklopJezik);
            ShraniTekmovalecCmd = new RelayCommand(async () => await ShraniTekmovalec());
            IzbrisiTekmovalecCmd = new RelayCommand(async () => await IzbrisiTekmovalec());
            NovTekmovalecCmd = new RelayCommand(NovTekmovalec);
            ShraniTekmovanjeCmd = new RelayCommand(async () => await ShraniTekmovanje());
            IzbrisiTekmovanjeCmd = new RelayCommand(async () => await IzbrisiTekmovanje());
            NovTekmovanjeCmd = new RelayCommand(NovTekmovanje);
            PocistiFilterCmd = new RelayCommand(PocistiFilter);
            IzvozCSVCmd = new RelayCommand(IzvozCSV);
            ShraniUporabnikCmd = new RelayCommand(async () => await ShraniUporabnik());
            IzbrisiUporabnikCmd = new RelayCommand(async () => await IzbrisiUporabnik());
            NovUporabnikCmd = new RelayCommand(NovUporabnik);
        }

        public async void NaloziVse()
        {
            StatusSporocilo = _jez.T("Nalaganje podatkov...", "Loading data...");

            // TEKMOVALCI
            try
            {
                var t = await _api.GetTekmovalciAsync();
                _vsiTekmovalci = t;
                FilterDrzave.Clear();
                FilterDrzave.Add("—");
                foreach (var d in t.Select(x => x.Drzava ?? "").Where(d => d != "").Distinct().OrderBy(d => d))
                    FilterDrzave.Add(d);
                _izbranaDrzava = "—";
                OnPropertyChanged(nameof(IzbranaDrzava));
                Tekmovalci.Clear();
                t.ForEach(x => Tekmovalci.Add(x));
                _skupajTekmovalcev = t.Count;
                OnPropertyChanged(nameof(StatSkupajTekmovalcev));
                OnPropertyChanged(nameof(RezultatiStevilka));
            }
            catch (Exception ex) { StatusSporocilo = $"Tekmovalci napaka: {ex.Message}"; return; }

            // TEKMOVANJA
            try
            {
                var tm = await _api.GetTekmovanjaAsync();
                Tekmovanja.Clear();
                tm.ForEach(x => Tekmovanja.Add(x));
                _skupajTekmovanj = tm.Count;
                OnPropertyChanged(nameof(StatSkupajTekmovanj));

                // Statistika po tipu — izračunaj iz naloženih tekmovanj
                StatPoTipih.Clear();
                foreach (var skupina in tm.GroupBy(x => x.Tip ?? "Neznano").OrderBy(g => g.Key))
                {
                    StatPoTipih.Add(new StatistikaRow
                    {
                        Tip = skupina.Key,
                        Tekmovanj = skupina.Count(),
                        Rezultatov = 0
                    });
                }
            }
            catch (Exception ex) { StatusSporocilo = $"Tekmovanja napaka: {ex.Message}"; return; }

            // REZULTATI
            try
            {
                var r = await _api.GetRezultatiAsync();
                Rezultati.Clear();
                r.ForEach(x => Rezultati.Add(x));
                _skupajRezultatov = r.Count;
                OnPropertyChanged(nameof(StatSkupajRezultatov));
            }
            catch (Exception ex) { StatusSporocilo = $"Rezultati napaka: {ex.Message}"; return; }

            // UPORABNIKI
            try
            {
                var u = await _api.GetUporabnikiAsync();
                Uporabniki.Clear();
                u.ForEach(x => Uporabniki.Add(x));
            }
            catch { /* Tiho */ }

            StatusSporocilo = _jez.T(
                $"Naloženo: {_skupajTekmovalcev} tekmovalcev, {_skupajTekmovanj} tekmovanj, {_skupajRezultatov} rezultatov.",
                $"Loaded: {_skupajTekmovalcev} athletes, {_skupajTekmovanj} races, {_skupajRezultatov} results.");
        }

        private void PocistiFilter()
        {
            IskalniNiz = "";
            IzbranaDrzava = "—";
        }

        private void IzvozCSV()
        {
            var dialog = new SaveFileDialog
            {
                Title = _jez.T("Shrani CSV", "Save CSV"),
                Filter = "CSV datoteke (*.csv)|*.csv",
                FileName = $"tekmovalci_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };
            if (dialog.ShowDialog() != true) return;
            try
            {
                using var sw = new StreamWriter(dialog.FileName, false, System.Text.Encoding.UTF8);
                sw.WriteLine("Id,Ime,Priimek,Starost,Kraj,Drzava,Poklic");
                foreach (var t in Tekmovalci)
                    sw.WriteLine($"{t.Id},{Csv(t.Ime)},{Csv(t.Priimek)},{t.Starost},{Csv(t.Kraj)},{Csv(t.Drzava)},{Csv(t.Poklic)}");
                StatusSporocilo = _jez.T(
                    $"Izvoženo {Tekmovalci.Count} tekmovalcev → {dialog.FileName}",
                    $"Exported {Tekmovalci.Count} athletes → {dialog.FileName}");
            }
            catch (Exception ex) { StatusSporocilo = ex.Message; }
        }

        private static string Csv(string? v)
        {
            if (string.IsNullOrEmpty(v)) return "";
            return v.Contains(',') || v.Contains('"') ? $"\"{v.Replace("\"", "\"\"")}\"" : v;
        }

        public void NapolniFormTekmovalec()
        {
            if (IzbranTekmovalec is null) return;
            TIme = IzbranTekmovalec.Ime ?? "";
            TPriimek = IzbranTekmovalec.Priimek ?? "";
            TStarost = IzbranTekmovalec.Starost?.ToString() ?? "";
            TKraj = IzbranTekmovalec.Kraj ?? "";
            TDrzava = IzbranTekmovalec.Drzava ?? "";
            TPoklic = IzbranTekmovalec.Poklic ?? "";
        }

        public void NapolniFormTekmovanje()
        {
            if (IzbranTekmovanje is null) return;
            TNaziv = IzbranTekmovanje.Naziv ?? "";
            TLokacija = IzbranTekmovanje.Lokacija ?? "";
            TDatum = IzbranTekmovanje.Datum?.ToString("yyyy-MM-dd") ?? "";
            TTip = IzbranTekmovanje.Tip ?? "";
        }

        public void NapolniFormUporabnik()
        {
            if (IzbranUporabnik is null) return;
            TUporabniskoIme = IzbranUporabnik.UporabniskoIme ?? "";
            TEmail = IzbranUporabnik.Email ?? "";
            TVloga = IzbranUporabnik.Vloga ?? "uporabnik";
            MainWindowRef?.ClearGeslo();
        }

        private void NovTekmovalec()
        {
            IzbranTekmovalec = null;
            TIme = TPriimek = TStarost = TKraj = TDrzava = TPoklic = "";
        }

        private void NovTekmovanje()
        {
            IzbranTekmovanje = null;
            TNaziv = TLokacija = TDatum = TTip = "";
        }

        private void NovUporabnik()
        {
            IzbranUporabnik = null;
            TUporabniskoIme = TEmail = "";
            TVloga = "uporabnik";
            MainWindowRef?.ClearGeslo();
        }

        private async System.Threading.Tasks.Task ShraniTekmovalec()
        {
            if (string.IsNullOrWhiteSpace(TIme) || string.IsNullOrWhiteSpace(TPriimek))
            {
                StatusSporocilo = _jez.T("Ime in priimek sta obvezna!", "Name and surname are required!");
                return;
            }
            try
            {
                var t = new Tekmovalec
                {
                    Ime = TIme,
                    Priimek = TPriimek,
                    Starost = int.TryParse(TStarost, out int s) ? s : null,
                    Kraj = TKraj,
                    Drzava = TDrzava,
                    Poklic = TPoklic
                };
                if (IzbranTekmovalec?.Id > 0)
                {
                    t.Id = IzbranTekmovalec.Id;
                    await _api.PutTekmovalecAsync(t.Id, t);
                    StatusSporocilo = _jez.T("Tekmovalec posodobljen.", "Athlete updated.");
                }
                else
                {
                    await _api.PostTekmovalecAsync(t);
                    StatusSporocilo = _jez.T("Tekmovalec dodan.", "Athlete added.");
                }
                NaloziVse();
            }
            catch (Exception ex) { StatusSporocilo = ex.Message; }
        }

        private async System.Threading.Tasks.Task IzbrisiTekmovalec()
        {
            if (IzbranTekmovalec is null) return;
            if (MessageBox.Show(_jez.T("Res izbrisati?", "Really delete?"), "",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            try
            {
                await _api.DeleteTekmovalecAsync(IzbranTekmovalec.Id);
                StatusSporocilo = _jez.T("Tekmovalec izbrisan.", "Athlete deleted.");
                NaloziVse();
            }
            catch (Exception ex) { StatusSporocilo = ex.Message; }
        }

        private async System.Threading.Tasks.Task ShraniTekmovanje()
        {
            if (string.IsNullOrWhiteSpace(TNaziv))
            {
                StatusSporocilo = _jez.T("Naziv tekmovanja je obvezen!", "Race name is required!");
                return;
            }
            try
            {
                var t = new Tekmovanje
                {
                    Naziv = TNaziv,
                    Lokacija = TLokacija,
                    Datum = DateTime.TryParse(TDatum, out var d) ? d : null,
                    Tip = TTip
                };
                if (IzbranTekmovanje?.Id > 0)
                {
                    t.Id = IzbranTekmovanje.Id;
                    await _api.PutTekmovanjeAsync(t.Id, t);
                    StatusSporocilo = _jez.T("Tekmovanje posodobljeno.", "Race updated.");
                }
                else
                {
                    await _api.PostTekmovanjeAsync(t);
                    StatusSporocilo = _jez.T("Tekmovanje dodano.", "Race added.");
                }
                NaloziVse();
            }
            catch (Exception ex) { StatusSporocilo = ex.Message; }
        }

        private async System.Threading.Tasks.Task IzbrisiTekmovanje()
        {
            if (IzbranTekmovanje is null) return;
            if (MessageBox.Show(_jez.T("Res izbrisati?", "Really delete?"), "",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            try
            {
                await _api.DeleteTekmovanjeAsync(IzbranTekmovanje.Id);
                StatusSporocilo = _jez.T("Tekmovanje izbrisano.", "Race deleted.");
                NaloziVse();
            }
            catch (Exception ex) { StatusSporocilo = ex.Message; }
        }

        private async System.Threading.Tasks.Task ShraniUporabnik()
        {
            if (string.IsNullOrWhiteSpace(TUporabniskoIme))
            {
                StatusSporocilo = _jez.T("Uporabniško ime je obvezno!", "Username is required!");
                return;
            }
            try
            {
                var geslo = MainWindowRef?.GetGeslo() ?? "";
                var u = new Uporabnik
                {
                    UporabniskoIme = TUporabniskoIme,
                    Email = TEmail,
                    Vloga = TVloga,
                    GesloHash = string.IsNullOrEmpty(geslo) ? null : geslo
                };
                if (IzbranUporabnik?.Id > 0)
                {
                    u.Id = IzbranUporabnik.Id;
                    await _api.PutUporabnikAsync(u.Id, u);
                    StatusSporocilo = _jez.T("Uporabnik posodobljen.", "User updated.");
                }
                else
                {
                    if (string.IsNullOrEmpty(geslo))
                    {
                        StatusSporocilo = _jez.T("Geslo je obvezno za novega uporabnika!", "Password required for new user!");
                        return;
                    }
                    await _api.PostUporabnikAsync(u);
                    StatusSporocilo = _jez.T("Uporabnik dodan.", "User added.");
                }
                NaloziVse();
                MainWindowRef?.ClearGeslo();
            }
            catch (Exception ex) { StatusSporocilo = ex.Message; }
        }

        private async System.Threading.Tasks.Task IzbrisiUporabnik()
        {
            if (IzbranUporabnik is null) return;
            if (MessageBox.Show(_jez.T("Res izbrisati uporabnika?", "Really delete user?"), "",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            try
            {
                await _api.DeleteUporabnikAsync(IzbranUporabnik.Id);
                StatusSporocilo = _jez.T("Uporabnik izbrisan.", "User deleted.");
                NaloziVse();
            }
            catch (Exception ex) { StatusSporocilo = ex.Message; }
        }

        private void PreklopJezik()
        {
            _jez.Preklopi();
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(NaslovAplikacije));
            OnPropertyChanged(nameof(JezikGumb));
            OnPropertyChanged(nameof(TabTekmovalci));
            OnPropertyChanged(nameof(TabTekmovanja));
            OnPropertyChanged(nameof(TabRezultati));
            OnPropertyChanged(nameof(TabStatistika));
            OnPropertyChanged(nameof(TabUporabniki));
            OnPropertyChanged(nameof(FormNaslovTekmovalec));
            OnPropertyChanged(nameof(FormNaslovTekmovanje));
            OnPropertyChanged(nameof(FormNaslovUporabnik));
            OnPropertyChanged(nameof(GumbShrani));
            OnPropertyChanged(nameof(GumbIzbrisi));
            OnPropertyChanged(nameof(GumbNov));
            OnPropertyChanged(nameof(GumbPocistiFilter));
            OnPropertyChanged(nameof(GumbIzvozCSV));
            OnPropertyChanged(nameof(IskalniPlaceholder));
            OnPropertyChanged(nameof(StatNaslov));
            OnPropertyChanged(nameof(StatLabelTekmovalcev));
            OnPropertyChanged(nameof(StatLabelTekmovanj));
            OnPropertyChanged(nameof(StatLabelRezultatov));
            OnPropertyChanged(nameof(StatPoTipihNaslov));
            OnPropertyChanged(nameof(RezultatiStevilka));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
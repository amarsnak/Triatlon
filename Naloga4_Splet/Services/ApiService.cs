// Services/ApiService.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Naloga4_Splet.Models;

namespace Naloga4_Splet.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private const string BASE = "https://localhost:7144/api";

        public ApiService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            _http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };
        }

        // ---------- TEKMOVALCI ----------
        public async Task<List<Tekmovalec>> GetTekmovalciAsync(int limit = 100)
        {
            var list = await _http.GetFromJsonAsync<List<Tekmovalec>>($"{BASE}/Tekmovalci?limit={limit}");
            return list ?? new();
        }

        public async Task<Tekmovalec?> GetTekmovalecAsync(int id)
        {
            try { return await _http.GetFromJsonAsync<Tekmovalec>($"{BASE}/Tekmovalci/{id}"); }
            catch { return null; }
        }

        public async Task<List<Tekmovalec>> IscitekmovalceAsync(string ime)
        {
            try
            {
                var list = await _http.GetFromJsonAsync<List<Tekmovalec>>($"{BASE}/Tekmovalci/iskanje?ime={Uri.EscapeDataString(ime)}");
                return list ?? new();
            }
            catch { return new(); }
        }

        // ---------- TEKMOVANJA ----------
        public async Task<List<Tekmovanje>> GetTekmovanjaAsync()
        {
            try
            {
                var list = await _http.GetFromJsonAsync<List<Tekmovanje>>($"{BASE}/Tekovanja");
                return list ?? new();
            }
            catch { return new(); }
        }

        public async Task<Tekmovanje?> GetTekmovanjeAsync(int id)
        {
            try { return await _http.GetFromJsonAsync<Tekmovanje>($"{BASE}/Tekovanja/{id}"); }
            catch { return null; }
        }

        // ---------- REZULTATI ----------
        public async Task<List<Rezultat>> GetRezultatiTekmovalcaAsync(int tekmovalecId)
        {
            try
            {
                var list = await _http.GetFromJsonAsync<List<Rezultat>>($"{BASE}/Rezultati/tekmovalec/{tekmovalecId}");
                return list ?? new();
            }
            catch { return new(); }
        }

        public async Task<List<LestvicaVrstica>> GetLestvicaAsync(int tekmovanjeId)
        {
            try
            {
                var list = await _http.GetFromJsonAsync<List<LestvicaVrstica>>($"{BASE}/Rezultati/lestvica/{tekmovanjeId}");
                return list ?? new();
            }
            catch { return new(); }
        }

        public async Task<StatistikaTekmovalca?> GetStatistikaTekmovalcaAsync(int tekmovalecId)
        {
            try { return await _http.GetFromJsonAsync<StatistikaTekmovalca>($"{BASE}/Rezultati/statistika/{tekmovalecId}"); }
            catch { return null; }
        }

        // Filter — vrne raw JSON string
        public async Task<string> GetFilterJsonAsync(string? drzava, string? kategorija, int? minStarost, int? maxStarost)
        {
            var q = new List<string>();
            if (!string.IsNullOrEmpty(drzava))     q.Add($"drzava={Uri.EscapeDataString(drzava)}");
            if (!string.IsNullOrEmpty(kategorija)) q.Add($"kategorija={Uri.EscapeDataString(kategorija)}");
            if (minStarost.HasValue)               q.Add($"minStarost={minStarost}");
            if (maxStarost.HasValue)               q.Add($"maxStarost={maxStarost}");
            var url = $"{BASE}/Rezultati/filter" + (q.Count > 0 ? "?" + string.Join("&", q) : "");
            try { return await _http.GetStringAsync(url); }
            catch { return "[]"; }
        }

        // ---------- KATEGORIJE ----------
        public async Task<List<Kategorija>> GetKategorijeAsync()
        {
            try
            {
                var list = await _http.GetFromJsonAsync<List<Kategorija>>($"{BASE}/Kategorije");
                return list ?? new();
            }
            catch { return new(); }
        }

        // ---------- UPORABNIKI ----------
        public async Task<Uporabnik?> LoginAsync(string ime, string geslo)
        {
            try
            {
                var res = await _http.PostAsJsonAsync($"{BASE}/Uporabniki/login",
                    new { UporabniskoIme = ime, Geslo = geslo });
                if (res.IsSuccessStatusCode)
                    return await res.Content.ReadFromJsonAsync<Uporabnik>();
                return null;
            }
            catch { return null; }
        }

        public async Task<bool> RegisterAsync(string ime, string email, string geslo)
        {
            try
            {
                var res = await _http.PostAsJsonAsync($"{BASE}/Uporabniki", new
                {
                    UporabniskoIme = ime,
                    Email          = email,
                    GesloHash      = geslo,
                    Vloga          = "uporabnik"
                });
                return res.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<List<Uporabnik>> GetUporabnikiAsync()
        {
            try
            {
                var list = await _http.GetFromJsonAsync<List<Uporabnik>>($"{BASE}/Uporabniki");
                return list ?? new();
            }
            catch { return new(); }
        }
    }
}

// Services/ApiService.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Naloga3_WPF.Models;

namespace Naloga3_WPF.Services
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
            _http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
        }

        // ---------- TEKMOVALCI ----------
        public async Task<List<Tekmovalec>> GetTekmovalciAsync()
        {
            var list = await _http.GetFromJsonAsync<List<Tekmovalec>>($"{BASE}/Tekmovalci?limit=5000");
            return list ?? new();
        }
        public async Task<Tekmovalec?> PostTekmovalecAsync(Tekmovalec t)
        {
            var res = await _http.PostAsJsonAsync($"{BASE}/Tekmovalci", t);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Tekmovalec>();
        }
        public async Task<Tekmovalec?> PutTekmovalecAsync(int id, Tekmovalec t)
        {
            var res = await _http.PutAsJsonAsync($"{BASE}/Tekmovalci/{id}", t);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Tekmovalec>();
        }
        public async Task DeleteTekmovalecAsync(int id)
        {
            var res = await _http.DeleteAsync($"{BASE}/Tekmovalci/{id}");
            res.EnsureSuccessStatusCode();
        }

        // ---------- TEKMOVANJA ----------
        public async Task<List<Tekmovanje>> GetTekmovanjaAsync()
        {
            var list = await _http.GetFromJsonAsync<List<Tekmovanje>>($"{BASE}/Tekovanja");
            return list ?? new();
        }
        public async Task<Tekmovanje?> PostTekmovanjeAsync(Tekmovanje t)
        {
            var res = await _http.PostAsJsonAsync($"{BASE}/Tekovanja", t);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Tekmovanje>();
        }
        public async Task<Tekmovanje?> PutTekmovanjeAsync(int id, Tekmovanje t)
        {
            var res = await _http.PutAsJsonAsync($"{BASE}/Tekovanja/{id}", t);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Tekmovanje>();
        }
        public async Task DeleteTekmovanjeAsync(int id)
        {
            var res = await _http.DeleteAsync($"{BASE}/Tekovanja/{id}");
            res.EnsureSuccessStatusCode();
        }

        // ---------- REZULTATI ----------
        public async Task<List<Rezultat>> GetRezultatiAsync()
        {
            var list = await _http.GetFromJsonAsync<List<Rezultat>>($"{BASE}/Rezultati?limit=5000000");
            return list ?? new();
        }

        // ---------- UPORABNIKI ----------
        public async Task<List<Uporabnik>> GetUporabnikiAsync()
        {
            try
            {
                var list = await _http.GetFromJsonAsync<List<Uporabnik>>($"{BASE}/Uporabniki");
                return list ?? new();
            }
            catch { return new(); }
        }
        public async Task<Uporabnik?> PostUporabnikAsync(Uporabnik u)
        {
            var res = await _http.PostAsJsonAsync($"{BASE}/Uporabniki", u);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Uporabnik>();
        }
        public async Task<Uporabnik?> PutUporabnikAsync(int id, Uporabnik u)
        {
            var res = await _http.PutAsJsonAsync($"{BASE}/Uporabniki/{id}", u);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<Uporabnik>();
        }
        public async Task DeleteUporabnikAsync(int id)
        {
            var res = await _http.DeleteAsync($"{BASE}/Uporabniki/{id}");
            res.EnsureSuccessStatusCode();
        }

        // ---------- STATISTIKA ----------
        public Task<List<StatistikaRow>> GetStatistikaAsync()
        {
            return Task.FromResult(new List<StatistikaRow>());
        }
    }
}

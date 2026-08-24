

using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Npgsql;

namespace Naloga1_Uvoz
{
    class Tekmovanje
    {
        public string Naziv { get; set; } = "";
        public string Lokacija { get; set; } = "";
        public string Tip { get; set; } = "";
        public string Leto { get; set; } = "";
        public List<Rezultat> Rezultati { get; set; } = new();
    }

    class Rezultat
    {
        public string Ime { get; set; } = "";
        public string UvrstevSpol { get; set; } = "";
        public string UvrstevKategorija { get; set; } = "";
        public string UvrstevSkupna { get; set; } = "";
        public string Bib { get; set; } = "";
        public string Kategorija { get; set; } = "";
        public string Starost { get; set; } = "";
        public string Kraj { get; set; } = "";
        public string Drzava { get; set; } = "";
        public string Poklic { get; set; } = "";
        public string Tocke { get; set; } = "";
        public string CasPlavanje { get; set; } = "";
        public string CasT1 { get; set; } = "";
        public string CasKolesarjenje { get; set; } = "";
        public string CasT2 { get; set; } = "";
        public string CasTek { get; set; } = "";
        public string CasSkupni { get; set; } = "";
    }

    static class Pomocnik
    {
        public static string PreveriManjkajoce(string vrednost)
        {
            if (string.IsNullOrWhiteSpace(vrednost) || vrednost.Trim() == "---")
                return "EMPTY";
            return vrednost.Trim();
        }

        public static (string lokacija, string leto) PridobitekLokacijeInLeta(string imeDatoteke)
        {
            string brezKoncnice = Path.GetFileNameWithoutExtension(imeDatoteke);
            string[] deli = brezKoncnice.Split('_');
            return (deli.Length > 1 ? deli[1] : "NEZNANO",
                    deli.Length > 2 ? deli[2] : "NEZNANO");
        }

        public static string PridobitekTipa(string pot)
        {
            if (pot.Contains("IRONMAN70.3") || pot.Contains("IRONMAN 70.3")) return "IRONMAN70.3";
            if (pot.Contains("Ultra") || pot.Contains("ultra")) return "Ultra-triatlon";
            return "IRONMAN";
        }
    }

    static class BralecCSV
    {
        public static List<Tekmovanje> PreberiteVse(string korenMapa)
        {
            var vsaTekmovanja = new List<Tekmovanje>();

            foreach (string podmapa in Directory.GetDirectories(korenMapa))
            {
                string csvMapa = Path.Combine(podmapa, "CSV");
                if (!Directory.Exists(csvMapa)) continue;

                string[] datoteke = Directory.GetFiles(csvMapa, "*.csv");
                string tip = Pomocnik.PridobitekTipa(podmapa);
                Console.WriteLine($"\n[{tip}] Najdenih datotek: {datoteke.Length}");

                foreach (string datoteka in datoteke)
                {
                    string imeDatoteke = Path.GetFileName(datoteka);
                    var (lokacija, leto) = Pomocnik.PridobitekLokacijeInLeta(imeDatoteke);

                    var tekmovanje = new Tekmovanje
                    {
                        Naziv = imeDatoteke, Lokacija = lokacija, Tip = tip, Leto = leto
                    };

                    try
                    {
                        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HasHeaderRecord = true, MissingFieldFound = null, BadDataFound = null
                        };

                        using var reader = new StreamReader(datoteka);
                        using var csv    = new CsvReader(reader, config);
                        csv.Read(); csv.ReadHeader();

                        while (csv.Read())
                        {
                            tekmovanje.Rezultati.Add(new Rezultat
                            {
                                Ime               = Pomocnik.PreveriManjkajoce(csv.GetField("name")        ?? ""),
                                UvrstevSpol       = Pomocnik.PreveriManjkajoce(csv.GetField("genderRank")  ?? ""),
                                UvrstevKategorija = Pomocnik.PreveriManjkajoce(csv.GetField("divRank")     ?? ""),
                                UvrstevSkupna     = Pomocnik.PreveriManjkajoce(csv.GetField("overallRank") ?? ""),
                                Bib               = Pomocnik.PreveriManjkajoce(csv.GetField("bib")        ?? ""),
                                Kategorija        = Pomocnik.PreveriManjkajoce(csv.GetField("division")   ?? ""),
                                Starost           = Pomocnik.PreveriManjkajoce(csv.GetField("age")        ?? ""),
                                Kraj              = Pomocnik.PreveriManjkajoce(csv.GetField("state")      ?? ""),
                                Drzava            = Pomocnik.PreveriManjkajoce(csv.GetField("country")    ?? ""),
                                Poklic            = Pomocnik.PreveriManjkajoce(csv.GetField("profession") ?? ""),
                                Tocke             = Pomocnik.PreveriManjkajoce(csv.GetField("points")     ?? ""),
                                CasPlavanje       = Pomocnik.PreveriManjkajoce(csv.GetField("swim")       ?? ""),
                                CasT1             = Pomocnik.PreveriManjkajoce(csv.GetField("t1")         ?? ""),
                                CasKolesarjenje   = Pomocnik.PreveriManjkajoce(csv.GetField("bike")       ?? ""),
                                CasT2             = Pomocnik.PreveriManjkajoce(csv.GetField("t2")         ?? ""),
                                CasTek            = Pomocnik.PreveriManjkajoce(csv.GetField("run")        ?? ""),
                                CasSkupni         = Pomocnik.PreveriManjkajoce(csv.GetField("overall")    ?? ""),
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  [!] Napaka: {ex.Message}");
                    }

                    vsaTekmovanja.Add(tekmovanje);
                    Console.WriteLine($"  Prebrano: {imeDatoteke} ({tekmovanje.Rezultati.Count} rezultatov)");
                }
            }
            return vsaTekmovanja;
        }
    }

    static class BazaPodatkov
    {
        private static readonly string ConnString =
            "Host=127.0.0.1;Port=5432;Database=triathlon;Username=postgres;Password=Geslo123";

        public static void ShraniVse(List<Tekmovanje> vsaTekmovanja)
        {
            using var conn = new NpgsqlConnection(ConnString);
            conn.Open();
            Console.WriteLine("\nPovezava z bazo uspešna.");

            int skupaj = 0;
            var zacetek = DateTime.Now;

            foreach (var t in vsaTekmovanja)
            {
                int tid = VstaviTekmovanje(conn, t);
                foreach (var r in t.Rezultati)
                {
                    int vid = VstaviTekmovalca(conn, r);
                    int kid = VstaviKategorijo(conn, r.Kategorija);
                    VstaviRezultat(conn, r, tid, vid, kid);
                    skupaj++;
                }
                Console.WriteLine($"  Shranjeno: {t.Naziv} ({t.Rezultati.Count} rezultatov)");
            }

            Console.WriteLine($"\n--- STATISTIKA ---");
            Console.WriteLine($"Tekmovanj:  {vsaTekmovanja.Count}");
            Console.WriteLine($"Rezultatov: {skupaj}");
            Console.WriteLine($"Čas uvoza:  {(DateTime.Now - zacetek).TotalSeconds:F2} s");
        }

        private static int VstaviTekmovanje(NpgsqlConnection conn, Tekmovanje t)
        {
            using var cmd = new NpgsqlCommand(@"
                INSERT INTO tekmovanje (naziv, lokacija, tip, datum)
                VALUES (@naziv, @lokacija, @tip, @datum)
                ON CONFLICT DO NOTHING RETURNING id", conn);

            cmd.Parameters.AddWithValue("naziv",    t.Naziv);
            cmd.Parameters.AddWithValue("lokacija", t.Lokacija);
            cmd.Parameters.AddWithValue("tip",      t.Tip);
            cmd.Parameters.AddWithValue("datum",
                int.TryParse(t.Leto, out int leto) ? (object)new DateTime(leto, 1, 1) : DBNull.Value);

            var result = cmd.ExecuteScalar();
            if (result == null)
            {
                using var sel = new NpgsqlCommand("SELECT id FROM tekmovanje WHERE naziv = @naziv", conn);
                sel.Parameters.AddWithValue("naziv", t.Naziv);
                return Convert.ToInt32(sel.ExecuteScalar());
            }
            return Convert.ToInt32(result);
        }

        private static int VstaviTekmovalca(NpgsqlConnection conn, Rezultat r)
        {
            string[] deli  = r.Ime.Split(' ');
            string ime     = deli.Length > 1 ? string.Join(" ", deli[1..]) : r.Ime;
            string priimek = deli.Length > 0 ? deli[0] : "EMPTY";

            using var cmd = new NpgsqlCommand(@"
                INSERT INTO tekmovalec (ime, priimek, starost, kraj, drzava, poklic)
                VALUES (@ime, @priimek, @starost, @kraj, @drzava, @poklic)
                RETURNING id", conn);

            cmd.Parameters.AddWithValue("ime",     ime);
            cmd.Parameters.AddWithValue("priimek", priimek);
            cmd.Parameters.AddWithValue("starost",
                int.TryParse(r.Starost, out int st) ? (object)st : DBNull.Value);
            cmd.Parameters.AddWithValue("kraj",   r.Kraj);
            cmd.Parameters.AddWithValue("drzava", r.Drzava);
            cmd.Parameters.AddWithValue("poklic", r.Poklic);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static int VstaviKategorijo(NpgsqlConnection conn, string naziv)
        {
            using var sel = new NpgsqlCommand("SELECT id FROM kategorija WHERE naziv = @naziv", conn);
            sel.Parameters.AddWithValue("naziv", naziv);
            var id = sel.ExecuteScalar();
            if (id != null) return Convert.ToInt32(id);

            using var ins = new NpgsqlCommand("INSERT INTO kategorija (naziv) VALUES (@naziv) RETURNING id", conn);
            ins.Parameters.AddWithValue("naziv", naziv);
            return Convert.ToInt32(ins.ExecuteScalar());
        }

        private static void VstaviRezultat(NpgsqlConnection conn, Rezultat r,
            int tid, int vid, int kid)
        {
            using var cmd = new NpgsqlCommand(@"
                INSERT INTO rezultat (
                    tekmovanje_id, tekmovalec_id, kategorija_id,
                    bib, uvrstitev_skupna, uvrstitev_spol, uvrstitev_kategorija,
                    cas_plavanje, cas_t1, cas_kolesarjenje, cas_t2, cas_tek, cas_skupni, tocke)
                VALUES (@tid, @vid, @kid, @bib, @uSkupna, @uSpol, @uKat,
                    @swim, @t1, @bike, @t2, @run, @overall, @tocke)", conn);

            cmd.Parameters.AddWithValue("tid",     tid);
            cmd.Parameters.AddWithValue("vid",     vid);
            cmd.Parameters.AddWithValue("kid",     kid);
            cmd.Parameters.AddWithValue("bib",     r.Bib);
            cmd.Parameters.AddWithValue("uSkupna",
                int.TryParse(r.UvrstevSkupna, out int us) ? (object)us : DBNull.Value);
            cmd.Parameters.AddWithValue("uSpol",
                int.TryParse(r.UvrstevSpol, out int usp) ? (object)usp : DBNull.Value);
            cmd.Parameters.AddWithValue("uKat",
                int.TryParse(r.UvrstevKategorija, out int uk) ? (object)uk : DBNull.Value);
            cmd.Parameters.AddWithValue("swim",    PretвориCas(r.CasPlavanje));
            cmd.Parameters.AddWithValue("t1",      PretвориCas(r.CasT1));
            cmd.Parameters.AddWithValue("bike",    PretвориCas(r.CasKolesarjenje));
            cmd.Parameters.AddWithValue("t2",      PretвориCas(r.CasT2));
            cmd.Parameters.AddWithValue("run",     PretвориCas(r.CasTek));
            cmd.Parameters.AddWithValue("overall", PretвориCas(r.CasSkupni));
            cmd.Parameters.AddWithValue("tocke",
                double.TryParse(r.Tocke, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out double to) ? (object)to : DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        private static object PretвориCas(string vrednost)
        {
            if (vrednost == "EMPTY") return DBNull.Value;
            if (TimeSpan.TryParseExact(vrednost, @"hh\:mm\:ss",
                    CultureInfo.InvariantCulture, out TimeSpan ts))
                return ts;
            return DBNull.Value;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string korenMapa = @"C:\Users\Aleks\source\repos\Triatlon\Race-Results\Race-Results";

            Console.WriteLine("=== OZRA Naloga 1: Uvoz triatlonskih podatkov ===\n");
            Console.WriteLine($"Iščem podatke v: {korenMapa}");

            if (!Directory.Exists(korenMapa))
            {
                Console.WriteLine("[!] Mapa ne obstaja! Preveri pot do podatkov.");
                return;
            }

            var zacetek = DateTime.Now;
            Console.WriteLine("\n--- Branje CSV datotek ---");
            var vsaTekmovanja = BralecCSV.PreberiteVse(korenMapa);
            Console.WriteLine($"\nSkupaj tekmovanj: {vsaTekmovanja.Count}");
            Console.WriteLine($"Čas branja: {(DateTime.Now - zacetek).TotalSeconds:F2} s");

            Console.WriteLine("\n--- Shranjevanje v bazo ---");
            BazaPodatkov.ShraniVse(vsaTekmovanja);

            Console.WriteLine("\nUvoz zaključen. Pritisni Enter za izhod.");
            Console.ReadLine();
        }
    }
}

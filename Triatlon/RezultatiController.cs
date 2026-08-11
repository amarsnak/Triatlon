// Controllers/RezultatiController.cs
using Microsoft.AspNetCore.Mvc;
using Triatlon.Data;
using Triatlon.Models;
using Npgsql;
using System.Text;

namespace Naloga2_REST.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RezultatiController : ControllerBase
    {
        private readonly DbHelper _db;
        public RezultatiController(DbHelper db) { _db = db; }

        // GET: api/rezultati?tekmovanjeId=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rezultat>>> GetVse([FromQuery] int? tekmovanjeId)
        {
            var seznam = new List<Rezultat>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            string sql = @"SELECT id, tekmovanje_id, tekmovalec_id, kategorija_id,
                               bib, uvrstitev_skupna, uvrstitev_spol, uvrstitev_kategorija,
                               cas_plavanje, cas_t1, cas_kolesarjenje, cas_t2, cas_tek, cas_skupni, tocke
                           FROM rezultat";
            if (tekmovanjeId.HasValue) sql += " WHERE tekmovanje_id = @tid";
            sql += " ORDER BY uvrstitev_skupna LIMIT 1000";
            using var cmd = new NpgsqlCommand(sql, conn);
            if (tekmovanjeId.HasValue) cmd.Parameters.AddWithValue("tid", tekmovanjeId.Value);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) seznam.Add(BeriRezultat(reader));
            return Ok(seznam);
        }

        // GET: api/rezultati/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Rezultat>> GetEnega(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                @"SELECT id, tekmovanje_id, tekmovalec_id, kategorija_id,
                         bib, uvrstitev_skupna, uvrstitev_spol, uvrstitev_kategorija,
                         cas_plavanje, cas_t1, cas_kolesarjenje, cas_t2, cas_tek, cas_skupni, tocke
                  FROM rezultat WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) return Ok(BeriRezultat(reader));
            return NotFound($"Rezultat z ID {id} ne obstaja.");
        }

        // GET: api/rezultati/tekmovalec/5
        [HttpGet("tekmovalec/{tekmovalecId}")]
        public async Task<ActionResult<IEnumerable<Rezultat>>> GetZaTekmovalca(int tekmovalecId)
        {
            var seznam = new List<Rezultat>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                @"SELECT id, tekmovanje_id, tekmovalec_id, kategorija_id,
                         bib, uvrstitev_skupna, uvrstitev_spol, uvrstitev_kategorija,
                         cas_plavanje, cas_t1, cas_kolesarjenje, cas_t2, cas_tek, cas_skupni, tocke
                  FROM rezultat WHERE tekmovalec_id = @vid ORDER BY id", conn);
            cmd.Parameters.AddWithValue("vid", tekmovalecId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) seznam.Add(BeriRezultat(reader));
            return Ok(seznam);
        }

        // GET: api/rezultati/lestvica/1
        [HttpGet("lestvica/{tekmovanjeId}")]
        public async Task<ActionResult> GetLestvica(int tekmovanjeId)
        {
            var seznam = new List<object>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                @"SELECT r.uvrstitev_skupna, t.ime, t.priimek, t.drzava,
                         r.cas_plavanje, r.cas_kolesarjenje, r.cas_tek, r.cas_skupni, r.tocke
                  FROM rezultat r
                  JOIN tekmovalec t ON r.tekmovalec_id = t.id
                  WHERE r.tekmovanje_id = @tid AND r.cas_skupni IS NOT NULL
                  ORDER BY r.cas_skupni ASC LIMIT 100", conn);
            cmd.Parameters.AddWithValue("tid", tekmovanjeId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                seznam.Add(new {
                    Uvrstitev       = reader.IsDBNull(0) ? null : (int?)reader.GetInt32(0),
                    Ime             = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Priimek         = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Drzava          = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CasPlavanje     = reader.IsDBNull(4) ? null : reader.GetFieldValue<TimeSpan>(4).ToString(@"hh\:mm\:ss"),
                    CasKolesarjenje = reader.IsDBNull(5) ? null : reader.GetFieldValue<TimeSpan>(5).ToString(@"hh\:mm\:ss"),
                    CasTek          = reader.IsDBNull(6) ? null : reader.GetFieldValue<TimeSpan>(6).ToString(@"hh\:mm\:ss"),
                    CasSkupni       = reader.IsDBNull(7) ? null : reader.GetFieldValue<TimeSpan>(7).ToString(@"hh\:mm\:ss"),
                    Tocke           = reader.IsDBNull(8) ? null : (double?)reader.GetDouble(8),
                });
            }
            return Ok(seznam);
        }

        // GET: api/rezultati/statistika/5
        [HttpGet("statistika/{tekmovalecId}")]
        public async Task<ActionResult> GetStatistika(int tekmovalecId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                @"SELECT COUNT(*),
                    MIN(cas_skupni),
                    AVG(EXTRACT(EPOCH FROM cas_skupni)),
                    MIN(cas_plavanje),
                    AVG(EXTRACT(EPOCH FROM cas_plavanje)),
                    MIN(cas_kolesarjenje),
                    AVG(EXTRACT(EPOCH FROM cas_kolesarjenje)),
                    MIN(cas_tek),
                    AVG(EXTRACT(EPOCH FROM cas_tek))
                  FROM rezultat
                  WHERE tekmovalec_id = @vid AND cas_skupni IS NOT NULL", conn);
            cmd.Parameters.AddWithValue("vid", tekmovalecId);
            using var r = await cmd.ExecuteReaderAsync();
            if (await r.ReadAsync())
            {
                return Ok(new {
                    TekmovalecId          = tekmovalecId,
                    SteviloNastopov       = r.GetInt64(0),
                    NajboljsiCas          = r.IsDBNull(1) ? null : r.GetFieldValue<TimeSpan>(1).ToString(@"hh\:mm\:ss"),
                    PovprecniCas          = r.IsDBNull(2) ? null : TimeSpan.FromSeconds(r.GetDouble(2)).ToString(@"hh\:mm\:ss"),
                    NajboljsiPlavanje     = r.IsDBNull(3) ? null : r.GetFieldValue<TimeSpan>(3).ToString(@"hh\:mm\:ss"),
                    PovprecniPlavanje     = r.IsDBNull(4) ? null : TimeSpan.FromSeconds(r.GetDouble(4)).ToString(@"hh\:mm\:ss"),
                    NajboljsiKolesarjenje = r.IsDBNull(5) ? null : r.GetFieldValue<TimeSpan>(5).ToString(@"hh\:mm\:ss"),
                    PovprecniKolesarjenje = r.IsDBNull(6) ? null : TimeSpan.FromSeconds(r.GetDouble(6)).ToString(@"hh\:mm\:ss"),
                    NajboljsiTek          = r.IsDBNull(7) ? null : r.GetFieldValue<TimeSpan>(7).ToString(@"hh\:mm\:ss"),
                    PovprecniTek          = r.IsDBNull(8) ? null : TimeSpan.FromSeconds(r.GetDouble(8)).ToString(@"hh\:mm\:ss"),
                });
            }
            return NotFound($"Tekmovalec z ID {tekmovalecId} nima rezultatov.");
        }

        // GET: api/rezultati/primerjava?id1=1&id2=2
        [HttpGet("primerjava")]
        public async Task<ActionResult> GetPrimerjava([FromQuery] int id1, [FromQuery] int id2)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            async Task<object?> Pridobi(int id)
            {
                using var cmd = new NpgsqlCommand(
                    @"SELECT t.ime, t.priimek, t.drzava,
                             COUNT(r.id),
                             MIN(r.cas_skupni),
                             AVG(EXTRACT(EPOCH FROM r.cas_skupni))
                      FROM tekmovalec t
                      LEFT JOIN rezultat r ON r.tekmovalec_id = t.id AND r.cas_skupni IS NOT NULL
                      WHERE t.id = @vid
                      GROUP BY t.ime, t.priimek, t.drzava", conn);
                cmd.Parameters.AddWithValue("vid", id);
                using var r = await cmd.ExecuteReaderAsync();
                if (!await r.ReadAsync()) return null;
                return new {
                    Id        = id,
                    Ime       = r.IsDBNull(0) ? null : r.GetString(0),
                    Priimek   = r.IsDBNull(1) ? null : r.GetString(1),
                    Drzava    = r.IsDBNull(2) ? null : r.GetString(2),
                    Nastopi   = r.GetInt64(3),
                    Najboljsi = r.IsDBNull(4) ? null : r.GetFieldValue<TimeSpan>(4).ToString(@"hh\:mm\:ss"),
                    Povprecje = r.IsDBNull(5) ? null : TimeSpan.FromSeconds(r.GetDouble(5)).ToString(@"hh\:mm\:ss"),
                };
            }

            var t1 = await Pridobi(id1);
            var t2 = await Pridobi(id2);
            if (t1 == null) return NotFound($"Tekmovalec z ID {id1} ne obstaja.");
            if (t2 == null) return NotFound($"Tekmovalec z ID {id2} ne obstaja.");
            return Ok(new { Tekmovalec1 = t1, Tekmovalec2 = t2 });
        }

        // GET: api/rezultati/filter?drzava=SLO&kategorija=M40
        [HttpGet("filter")]
        public async Task<ActionResult> GetFilter(
            [FromQuery] string? drzava,
            [FromQuery] string? kategorija,
            [FromQuery] int? minStarost,
            [FromQuery] int? maxStarost)
        {
            var seznam = new List<object>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            var sql = new StringBuilder(
                @"SELECT r.id, t.ime, t.priimek, t.drzava, t.starost,
                         k.naziv, r.uvrstitev_skupna, r.cas_skupni
                  FROM rezultat r
                  JOIN tekmovalec t ON r.tekmovalec_id = t.id
                  LEFT JOIN kategorija k ON r.kategorija_id = k.id
                  WHERE 1=1");
            if (!string.IsNullOrWhiteSpace(drzava))    sql.Append(" AND LOWER(t.drzava) = LOWER(@drzava)");
            if (!string.IsNullOrWhiteSpace(kategorija)) sql.Append(" AND LOWER(k.naziv) LIKE LOWER(@kategorija)");
            if (minStarost.HasValue) sql.Append(" AND t.starost >= @minStarost");
            if (maxStarost.HasValue) sql.Append(" AND t.starost <= @maxStarost");
            sql.Append(" ORDER BY r.cas_skupni ASC LIMIT 500");
            using var cmd = new NpgsqlCommand(sql.ToString(), conn);
            if (!string.IsNullOrWhiteSpace(drzava))    cmd.Parameters.AddWithValue("drzava", drzava);
            if (!string.IsNullOrWhiteSpace(kategorija)) cmd.Parameters.AddWithValue("kategorija", $"%{kategorija}%");
            if (minStarost.HasValue) cmd.Parameters.AddWithValue("minStarost", minStarost.Value);
            if (maxStarost.HasValue) cmd.Parameters.AddWithValue("maxStarost", maxStarost.Value);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                seznam.Add(new {
                    Id         = reader.GetInt32(0),
                    Ime        = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Priimek    = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Drzava     = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Starost    = reader.IsDBNull(4) ? null : (int?)reader.GetInt32(4),
                    Kategorija = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Uvrstitev  = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                    CasSkupni  = reader.IsDBNull(7) ? null : reader.GetFieldValue<TimeSpan>(7).ToString(@"hh\:mm\:ss"),
                });
            }
            return Ok(seznam);
        }

        // GET: api/rezultati/izvoz/1 — vrne CSV datoteko
        [HttpGet("izvoz/{tekmovanjeId}")]
        public async Task<IActionResult> GetIzvozCSV(int tekmovanjeId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                @"SELECT t.ime, t.priimek, t.drzava, t.starost,
                         r.bib, r.uvrstitev_skupna, r.cas_plavanje,
                         r.cas_kolesarjenje, r.cas_tek, r.cas_skupni, r.tocke
                  FROM rezultat r
                  JOIN tekmovalec t ON r.tekmovalec_id = t.id
                  WHERE r.tekmovanje_id = @tid
                  ORDER BY r.uvrstitev_skupna", conn);
            cmd.Parameters.AddWithValue("tid", tekmovanjeId);
            var sb = new StringBuilder();
            sb.AppendLine("Ime,Priimek,Drzava,Starost,Bib,Uvrstitev,Plavanje,Kolesarjenje,Tek,Skupni cas,Tocke");
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                sb.AppendLine(string.Join(",",
                    reader.IsDBNull(0)  ? "" : reader.GetString(0),
                    reader.IsDBNull(1)  ? "" : reader.GetString(1),
                    reader.IsDBNull(2)  ? "" : reader.GetString(2),
                    reader.IsDBNull(3)  ? "" : reader.GetInt32(3).ToString(),
                    reader.IsDBNull(4)  ? "" : reader.GetString(4),
                    reader.IsDBNull(5)  ? "" : reader.GetInt32(5).ToString(),
                    reader.IsDBNull(6)  ? "" : reader.GetFieldValue<TimeSpan>(6).ToString(@"hh\:mm\:ss"),
                    reader.IsDBNull(7)  ? "" : reader.GetFieldValue<TimeSpan>(7).ToString(@"hh\:mm\:ss"),
                    reader.IsDBNull(8)  ? "" : reader.GetFieldValue<TimeSpan>(8).ToString(@"hh\:mm\:ss"),
                    reader.IsDBNull(9)  ? "" : reader.GetFieldValue<TimeSpan>(9).ToString(@"hh\:mm\:ss"),
                    reader.IsDBNull(10) ? "" : reader.GetDouble(10).ToString()
                ));
            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"rezultati_{tekmovanjeId}.csv");
        }

        // POST: api/rezultati
        [HttpPost]
        public async Task<ActionResult<Rezultat>> Dodaj([FromBody] Rezultat r)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                @"INSERT INTO rezultat (tekmovanje_id, tekmovalec_id, kategorija_id,
                    bib, uvrstitev_skupna, uvrstitev_spol, uvrstitev_kategorija,
                    cas_plavanje, cas_t1, cas_kolesarjenje, cas_t2, cas_tek, cas_skupni, tocke)
                  VALUES (@tid, @vid, @kid, @bib, @uSkupna, @uSpol, @uKat,
                    @swim, @t1, @bike, @t2, @run, @overall, @tocke)
                  RETURNING id", conn);
            cmd.Parameters.AddWithValue("tid",     r.TekovanjeId);
            cmd.Parameters.AddWithValue("vid",     r.TekmovalecId);
            cmd.Parameters.AddWithValue("kid",     (object?)r.KategorijaId      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("bib",     (object?)r.Bib               ?? DBNull.Value);
            cmd.Parameters.AddWithValue("uSkupna", (object?)r.UvrstevSkupna     ?? DBNull.Value);
            cmd.Parameters.AddWithValue("uSpol",   (object?)r.UvrstevSpol       ?? DBNull.Value);
            cmd.Parameters.AddWithValue("uKat",    (object?)r.UvrstevKategorija ?? DBNull.Value);
            cmd.Parameters.AddWithValue("swim",    (object?)r.CasPlavanje       ?? DBNull.Value);
            cmd.Parameters.AddWithValue("t1",      (object?)r.CasT1             ?? DBNull.Value);
            cmd.Parameters.AddWithValue("bike",    (object?)r.CasKolesarjenje   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("t2",      (object?)r.CasT2             ?? DBNull.Value);
            cmd.Parameters.AddWithValue("run",     (object?)r.CasTek            ?? DBNull.Value);
            cmd.Parameters.AddWithValue("overall", (object?)r.CasSkupni         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("tocke",   (object?)r.Tocke             ?? DBNull.Value);
            r.Id = (int)(await cmd.ExecuteScalarAsync())!;
            return CreatedAtAction(nameof(GetEnega), new { id = r.Id }, r);
        }

        // PUT: api/rezultati/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Uredi(int id, [FromBody] Rezultat r)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                @"UPDATE rezultat SET bib=@bib, uvrstitev_skupna=@uSkupna,
                    uvrstitev_spol=@uSpol, uvrstitev_kategorija=@uKat,
                    cas_plavanje=@swim, cas_t1=@t1, cas_kolesarjenje=@bike,
                    cas_t2=@t2, cas_tek=@run, cas_skupni=@overall, tocke=@tocke
                  WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("id",      id);
            cmd.Parameters.AddWithValue("bib",     (object?)r.Bib               ?? DBNull.Value);
            cmd.Parameters.AddWithValue("uSkupna", (object?)r.UvrstevSkupna     ?? DBNull.Value);
            cmd.Parameters.AddWithValue("uSpol",   (object?)r.UvrstevSpol       ?? DBNull.Value);
            cmd.Parameters.AddWithValue("uKat",    (object?)r.UvrstevKategorija ?? DBNull.Value);
            cmd.Parameters.AddWithValue("swim",    (object?)r.CasPlavanje       ?? DBNull.Value);
            cmd.Parameters.AddWithValue("t1",      (object?)r.CasT1             ?? DBNull.Value);
            cmd.Parameters.AddWithValue("bike",    (object?)r.CasKolesarjenje   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("t2",      (object?)r.CasT2             ?? DBNull.Value);
            cmd.Parameters.AddWithValue("run",     (object?)r.CasTek            ?? DBNull.Value);
            cmd.Parameters.AddWithValue("overall", (object?)r.CasSkupni         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("tocke",   (object?)r.Tocke             ?? DBNull.Value);
            int vrstice = await cmd.ExecuteNonQueryAsync();
            if (vrstice == 0) return NotFound($"Rezultat z ID {id} ne obstaja.");
            return NoContent();
        }

        // DELETE: api/rezultati/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Izbrisi(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("DELETE FROM rezultat WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("id", id);
            int vrstice = await cmd.ExecuteNonQueryAsync();
            if (vrstice == 0) return NotFound($"Rezultat z ID {id} ne obstaja.");
            return NoContent();
        }

        private static Rezultat BeriRezultat(NpgsqlDataReader r) => new()
        {
            Id                = r.GetInt32(0),
            TekovanjeId       = r.GetInt32(1),
            TekmovalecId      = r.GetInt32(2),
            KategorijaId      = r.IsDBNull(3)  ? null : r.GetInt32(3),
            Bib               = r.IsDBNull(4)  ? null : r.GetString(4),
            UvrstevSkupna     = r.IsDBNull(5)  ? null : r.GetInt32(5),
            UvrstevSpol       = r.IsDBNull(6)  ? null : r.GetInt32(6),
            UvrstevKategorija = r.IsDBNull(7)  ? null : r.GetInt32(7),
            CasPlavanje       = r.IsDBNull(8)  ? null : r.GetFieldValue<TimeSpan>(8),
            CasT1             = r.IsDBNull(9)  ? null : r.GetFieldValue<TimeSpan>(9),
            CasKolesarjenje   = r.IsDBNull(10) ? null : r.GetFieldValue<TimeSpan>(10),
            CasT2             = r.IsDBNull(11) ? null : r.GetFieldValue<TimeSpan>(11),
            CasTek            = r.IsDBNull(12) ? null : r.GetFieldValue<TimeSpan>(12),
            CasSkupni         = r.IsDBNull(13) ? null : r.GetFieldValue<TimeSpan>(13),
            Tocke             = r.IsDBNull(14) ? null : r.GetDouble(14),
        };
    }
}

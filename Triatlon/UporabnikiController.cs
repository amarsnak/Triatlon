// Controllers/UporabnikiController.cs
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Triatlon.Data;
using Triatlon.Models;

namespace Triatlon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UporabnikiController : ControllerBase
    {
        private readonly DbHelper _db;

        public UporabnikiController(DbHelper db)
        {
            _db = db;
        }

        // GET: api/uporabniki
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Uporabnik>>> GetVse()
        {
            var seznam = new List<Uporabnik>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "SELECT id, uporabnisko_ime, geslo_hash, vloga, email FROM uporabnik ORDER BY id", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                seznam.Add(BeriUporabnika(reader));

            return Ok(seznam);
        }

        // GET: api/uporabniki/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Uporabnik>> GetEnega(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "SELECT id, uporabnisko_ime, geslo_hash, vloga, email FROM uporabnik WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                return Ok(BeriUporabnika(reader));

            return NotFound($"Uporabnik z ID {id} ne obstaja.");
        }

        // POST: api/uporabniki
        [HttpPost]
        public async Task<ActionResult<Uporabnik>> Dodaj([FromBody] Uporabnik u)
        {
            if (string.IsNullOrWhiteSpace(u.UporabniskoIme))
                return BadRequest("Uporabniško ime je obvezno.");

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                @"INSERT INTO uporabnik (uporabnisko_ime, geslo_hash, vloga, email)
                  VALUES (@ime, @geslo, @vloga, @email)
                  RETURNING id", conn);

            cmd.Parameters.AddWithValue("ime",   u.UporabniskoIme);
            cmd.Parameters.AddWithValue("geslo", (object?)u.GesloHash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("vloga", (object?)u.Vloga    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("email", (object?)u.Email    ?? DBNull.Value);

            u.Id = (int)(await cmd.ExecuteScalarAsync())!;
            return CreatedAtAction(nameof(GetEnega), new { id = u.Id }, u);
        }

        // PUT: api/uporabniki/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Uredi(int id, [FromBody] Uporabnik u)
        {
            if (string.IsNullOrWhiteSpace(u.UporabniskoIme))
                return BadRequest("Uporabniško ime je obvezno.");

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            // Če je geslo prazno, ga ne posodabljamo
            string sql = string.IsNullOrWhiteSpace(u.GesloHash)
                ? @"UPDATE uporabnik SET uporabnisko_ime=@ime, vloga=@vloga, email=@email WHERE id=@id"
                : @"UPDATE uporabnik SET uporabnisko_ime=@ime, geslo_hash=@geslo, vloga=@vloga, email=@email WHERE id=@id";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id",    id);
            cmd.Parameters.AddWithValue("ime",   u.UporabniskoIme);
            cmd.Parameters.AddWithValue("vloga", (object?)u.Vloga ?? DBNull.Value);
            cmd.Parameters.AddWithValue("email", (object?)u.Email ?? DBNull.Value);
            if (!string.IsNullOrWhiteSpace(u.GesloHash))
                cmd.Parameters.AddWithValue("geslo", u.GesloHash);

            int vrstice = await cmd.ExecuteNonQueryAsync();
            if (vrstice == 0)
                return NotFound($"Uporabnik z ID {id} ne obstaja.");

            return NoContent();
        }

        // DELETE: api/uporabniki/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Izbrisi(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "DELETE FROM uporabnik WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);

            int vrstice = await cmd.ExecuteNonQueryAsync();
            if (vrstice == 0)
                return NotFound($"Uporabnik z ID {id} ne obstaja.");

            return NoContent();
        }

        private static Uporabnik BeriUporabnika(NpgsqlDataReader r) => new()
        {
            Id              = r.GetInt32(0),
            UporabniskoIme  = r.IsDBNull(1) ? null : r.GetString(1),
            GesloHash       = r.IsDBNull(2) ? null : r.GetString(2),
            Vloga           = r.IsDBNull(3) ? null : r.GetString(3),
            Email           = r.IsDBNull(4) ? null : r.GetString(4),
        };
    }
}

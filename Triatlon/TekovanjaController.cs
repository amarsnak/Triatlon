// Controllers/TekovanjaController.cs
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Triatlon.Data;
using Triatlon.Models;

namespace Triatlon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TekovanjaController : ControllerBase
    {
        private readonly DbHelper _db;

        public TekovanjaController(DbHelper db)
        {
            _db = db;
        }

        // GET: api/tekovnja
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tekmovanje>>> GetVse()
        {
            var seznam = new List<Tekmovanje>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "SELECT id, naziv, lokacija, tip, datum, uporabnik_id FROM tekmovanje ORDER BY datum DESC", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                seznam.Add(BeriTekmovanje(reader));

            return Ok(seznam);
        }

        // GET: api/tekovnja/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tekmovanje>> GetEnega(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "SELECT id, naziv, lokacija, tip, datum, uporabnik_id FROM tekmovanje WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                return Ok(BeriTekmovanje(reader));

            return NotFound($"Tekmovanje z ID {id} ne obstaja.");
        }

        // GET: api/tekovnja/tip/IRONMAN
        [HttpGet("tip/{tip}")]
        public async Task<ActionResult<IEnumerable<Tekmovanje>>> GetPoTipu(string tip)
        {
            var seznam = new List<Tekmovanje>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "SELECT id, naziv, lokacija, tip, datum, uporabnik_id FROM tekmovanje WHERE tip = @tip ORDER BY datum DESC", conn);
            cmd.Parameters.AddWithValue("tip", tip);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                seznam.Add(BeriTekmovanje(reader));

            return Ok(seznam);
        }

        // POST: api/tekovnja
        [HttpPost]
        public async Task<ActionResult<Tekmovanje>> Dodaj([FromBody] Tekmovanje t)
        {
            if (string.IsNullOrWhiteSpace(t.Naziv))
                return BadRequest("Naziv tekmovanja je obvezen.");

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                @"INSERT INTO tekmovanje (naziv, lokacija, tip, datum, uporabnik_id)
                  VALUES (@naziv, @lokacija, @tip, @datum, @uid)
                  RETURNING id", conn);

            cmd.Parameters.AddWithValue("naziv",    t.Naziv);
            cmd.Parameters.AddWithValue("lokacija", (object?)t.Lokacija ?? DBNull.Value);
            cmd.Parameters.AddWithValue("tip",      (object?)t.Tip ?? DBNull.Value);
            cmd.Parameters.AddWithValue("datum",    (object?)t.Datum ?? DBNull.Value);
            cmd.Parameters.AddWithValue("uid",      (object?)t.UporabnikId ?? DBNull.Value);

            t.Id = (int)(await cmd.ExecuteScalarAsync())!;
            return CreatedAtAction(nameof(GetEnega), new { id = t.Id }, t);
        }

        // PUT: api/tekovnja/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Uredi(int id, [FromBody] Tekmovanje t)
        {
            if (string.IsNullOrWhiteSpace(t.Naziv))
                return BadRequest("Naziv tekmovanja je obvezen.");

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                @"UPDATE tekmovanje SET
                    naziv = @naziv, lokacija = @lokacija, tip = @tip,
                    datum = @datum, uporabnik_id = @uid
                  WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("id",       id);
            cmd.Parameters.AddWithValue("naziv",    t.Naziv);
            cmd.Parameters.AddWithValue("lokacija", (object?)t.Lokacija ?? DBNull.Value);
            cmd.Parameters.AddWithValue("tip",      (object?)t.Tip ?? DBNull.Value);
            cmd.Parameters.AddWithValue("datum",    (object?)t.Datum ?? DBNull.Value);
            cmd.Parameters.AddWithValue("uid",      (object?)t.UporabnikId ?? DBNull.Value);

            int vrstice = await cmd.ExecuteNonQueryAsync();
            if (vrstice == 0)
                return NotFound($"Tekmovanje z ID {id} ne obstaja.");

            return NoContent();
        }

        // DELETE: api/tekovnja/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Izbrisi(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "DELETE FROM tekmovanje WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);

            int vrstice = await cmd.ExecuteNonQueryAsync();
            if (vrstice == 0)
                return NotFound($"Tekmovanje z ID {id} ne obstaja.");

            return NoContent();
        }

        private static Tekmovanje BeriTekmovanje(NpgsqlDataReader r) => new()
        {
            Id          = r.GetInt32(0),
            Naziv       = r.IsDBNull(1) ? null : r.GetString(1),
            Lokacija    = r.IsDBNull(2) ? null : r.GetString(2),
            Tip         = r.IsDBNull(3) ? null : r.GetString(3),
            Datum       = r.IsDBNull(4) ? null : r.GetDateTime(4),
            UporabnikId = r.IsDBNull(5) ? null : r.GetInt32(5),
        };
    }
}

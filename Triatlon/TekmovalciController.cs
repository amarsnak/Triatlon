// Controllers/TekmovalciController.cs
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Triatlon.Data;
using Triatlon.Models;

namespace Triatlon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TekmovalciController : ControllerBase
    {
        private readonly DbHelper _db;

        public TekmovalciController(DbHelper db)
        {
            _db = db;
        }

        // GET: api/tekmovalci
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tekmovalec>>> GetVse([FromQuery] int limit = 200)
        {
            var seznam = new List<Tekmovalec>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "SELECT id, ime, priimek, starost, kraj, drzava, poklic FROM tekmovalec ORDER BY id LIMIT @limit", conn);
            cmd.Parameters.AddWithValue("limit", limit);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                seznam.Add(BeriTekmovalca(reader));
            }
            return Ok(seznam);
        }

        // GET: api/tekmovalci/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tekmovalec>> GetEnega(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "SELECT id, ime, priimek, starost, kraj, drzava, poklic FROM tekmovalec WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                return Ok(BeriTekmovalca(reader));

            return NotFound($"Tekmovalec z ID {id} ne obstaja.");
        }

        // GET: api/tekmovalci/iskanje?ime=John
        [HttpGet("iskanje")]
        public async Task<ActionResult<IEnumerable<Tekmovalec>>> Iskanje([FromQuery] string ime)
        {
            var seznam = new List<Tekmovalec>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                @"SELECT id, ime, priimek, starost, kraj, drzava, poklic 
                  FROM tekmovalec 
                  WHERE LOWER(ime) LIKE LOWER(@ime) OR LOWER(priimek) LIKE LOWER(@ime)
                  ORDER BY priimek, ime", conn);
            cmd.Parameters.AddWithValue("ime", $"%{ime}%");
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                seznam.Add(BeriTekmovalca(reader));

            return Ok(seznam);
        }

        // POST: api/tekmovalci
        [HttpPost]
        public async Task<ActionResult<Tekmovalec>> Dodaj([FromBody] Tekmovalec t)
        {
            if (string.IsNullOrWhiteSpace(t.Ime) || string.IsNullOrWhiteSpace(t.Priimek))
                return BadRequest("Ime in priimek sta obvezna.");

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                @"INSERT INTO tekmovalec (ime, priimek, starost, kraj, drzava, poklic)
                  VALUES (@ime, @priimek, @starost, @kraj, @drzava, @poklic)
                  RETURNING id", conn);

            cmd.Parameters.AddWithValue("ime",     t.Ime);
            cmd.Parameters.AddWithValue("priimek", t.Priimek);
            cmd.Parameters.AddWithValue("starost", (object?)t.Starost ?? DBNull.Value);
            cmd.Parameters.AddWithValue("kraj",    (object?)t.Kraj ?? DBNull.Value);
            cmd.Parameters.AddWithValue("drzava",  (object?)t.Drzava ?? DBNull.Value);
            cmd.Parameters.AddWithValue("poklic",  (object?)t.Poklic ?? DBNull.Value);

            t.Id = (int)(await cmd.ExecuteScalarAsync())!;
            return CreatedAtAction(nameof(GetEnega), new { id = t.Id }, t);
        }

        // PUT: api/tekmovalci/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Uredi(int id, [FromBody] Tekmovalec t)
        {
            if (string.IsNullOrWhiteSpace(t.Ime) || string.IsNullOrWhiteSpace(t.Priimek))
                return BadRequest("Ime in priimek sta obvezna.");

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                @"UPDATE tekmovalec SET
                    ime = @ime, priimek = @priimek, starost = @starost,
                    kraj = @kraj, drzava = @drzava, poklic = @poklic
                  WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("id",      id);
            cmd.Parameters.AddWithValue("ime",     t.Ime);
            cmd.Parameters.AddWithValue("priimek", t.Priimek);
            cmd.Parameters.AddWithValue("starost", (object?)t.Starost ?? DBNull.Value);
            cmd.Parameters.AddWithValue("kraj",    (object?)t.Kraj ?? DBNull.Value);
            cmd.Parameters.AddWithValue("drzava",  (object?)t.Drzava ?? DBNull.Value);
            cmd.Parameters.AddWithValue("poklic",  (object?)t.Poklic ?? DBNull.Value);

            int vrstice = await cmd.ExecuteNonQueryAsync();
            if (vrstice == 0)
                return NotFound($"Tekmovalec z ID {id} ne obstaja.");

            return NoContent();
        }

        // DELETE: api/tekmovalci/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Izbrisi(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "DELETE FROM tekmovalec WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);

            int vrstice = await cmd.ExecuteNonQueryAsync();
            if (vrstice == 0)
                return NotFound($"Tekmovalec z ID {id} ne obstaja.");

            return NoContent();
        }

        private static Tekmovalec BeriTekmovalca(NpgsqlDataReader r) => new()
        {
            Id      = r.GetInt32(0),
            Ime     = r.IsDBNull(1) ? null : r.GetString(1),
            Priimek = r.IsDBNull(2) ? null : r.GetString(2),
            Starost = r.IsDBNull(3) ? null : r.GetInt32(3),
            Kraj    = r.IsDBNull(4) ? null : r.GetString(4),
            Drzava  = r.IsDBNull(5) ? null : r.GetString(5),
            Poklic  = r.IsDBNull(6) ? null : r.GetString(6),
        };
    }
}

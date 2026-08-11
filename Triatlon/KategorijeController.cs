// Controllers/KategorijeController.cs
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Triatlon.Data;
using Triatlon.Models;

namespace Triatlon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KategorijeController : ControllerBase
    {
        private readonly DbHelper _db;

        public KategorijeController(DbHelper db)
        {
            _db = db;
        }

        // GET: api/kategorije
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Kategorija>>> GetVse()
        {
            var seznam = new List<Kategorija>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "SELECT id, naziv, spol, min_starost, max_starost FROM kategorija ORDER BY naziv", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                seznam.Add(new Kategorija
                {
                    Id         = reader.GetInt32(0),
                    Naziv      = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Spol       = reader.IsDBNull(2) ? null : reader.GetString(2),
                    MinStarost = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    MaxStarost = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                });
            }
            return Ok(seznam);
        }

        // GET: api/kategorije/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Kategorija>> GetEnega(int id)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "SELECT id, naziv, spol, min_starost, max_starost FROM kategorija WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return Ok(new Kategorija
                {
                    Id         = reader.GetInt32(0),
                    Naziv      = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Spol       = reader.IsDBNull(2) ? null : reader.GetString(2),
                    MinStarost = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    MaxStarost = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                });
            }
            return NotFound($"Kategorija z ID {id} ne obstaja.");
        }
    }
}

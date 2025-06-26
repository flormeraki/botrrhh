using Microsoft.AspNetCore.Mvc;
using System.Data.OleDb;

namespace BotRRHH.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BotController : ControllerBase
    {
        private readonly string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Data\BotRRHH.accdb;";

        [HttpPost("consulta")]
        public IActionResult Consulta([FromBody] string pregunta, [FromQuery] string nivel = "publico")
        {
            try
            {
                string respuesta = BuscarRespuesta(pregunta.ToLower(), nivel.ToLower());
                return Ok(new { respuesta });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private string BuscarRespuesta(string pregunta, string nivelAcceso)
        {
            using var connection = new OleDbConnection(connectionString);
            connection.Open();
            string query = "SELECT respuesta FROM Faqs WHERE nivel_acceso IN ('publico', ?) AND ? LIKE '*' & palabra_clave & '*'";
            using var command = new OleDbCommand(query, connection);
            command.Parameters.AddWithValue("nivelAcceso", nivelAcceso);
            command.Parameters.AddWithValue("pregunta", pregunta);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return reader.GetString(0);
            }
            return "No entendí tu consulta o no tenés permisos para ver la respuesta.";
        }
    }
}


/*using Microsoft.AspNetCore.Mvc;
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
}*/

// Usamos los paquetes necesarios para:
// - ASP.NET Core MVC (para el controlador y endpoints)
// - SqlClient (para conectarnos a una base de datos SQL Server)
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace BotRRHH.Controllers
{
    // Esta clase es un controlador de una API REST.
    // [ApiController] le dice a ASP.NET que valide automáticamente los datos de entrada.
    // [Route("[controller]")] define que este controlador responde en la URL /bot (por el nombre BotController).
    [ApiController]
    [Route("[controller]")]
    public class BotController : ControllerBase
    {
        // Cadena de conexión a la base de datos SQL Server
        //private readonly string connectionString = @"Server=TU_SERVIDOR_SQL;Database=TuBaseRRHH;Trusted_Connection=True;";
        private readonly string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=TuBaseRRHH;Trusted_Connection=True;";

        // Este método responde a POST /bot/consulta
        // Recibe un JSON con la pregunta del usuario y si está logueado.
        [HttpPost("consulta")]
        public IActionResult Consulta([FromBody] ConsultaRequest request)
        {
            // Llama a un método privado que busca la respuesta en la base de datos
            string respuesta = ObtenerRespuesta(request.Pregunta, request.EstaLogueado);

            // Devuelve la respuesta en formato JSON
            return Ok(new { respuesta });
        }

        // Este método se conecta a la base y busca una respuesta en la tabla FaqBot
        private string ObtenerRespuesta(string pregunta, bool estaLogueado)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Consulta SQL que busca una pregunta cuya palabra clave coincida con alguna de las palabras extraídas
                // También verifica el nivel de acceso: si es público o si el usuario está logueado
                string sql = @"
                    SELECT TOP 1 pregunta, respuesta
                    FROM FaqBot
                    WHERE palabra_clave IN (@keyword1, @keyword2, @keyword3)
                    AND (nivel_acceso = 'Publico' OR @acceso = 1)
                    ORDER BY id";

                // Se extraen las palabras clave de la pregunta para usarlas en la búsqueda
                string[] keywords = ObtenerKeywords(pregunta);

                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    // Asignamos hasta 3 palabras clave como parámetros
                    cmd.Parameters.AddWithValue("@keyword1", keywords.Length > 0 ? keywords[0] : "");
                    cmd.Parameters.AddWithValue("@keyword2", keywords.Length > 1 ? keywords[1] : "");
                    cmd.Parameters.AddWithValue("@keyword3", keywords.Length > 2 ? keywords[2] : "");
                    cmd.Parameters.AddWithValue("@acceso", estaLogueado ? 1 : 0);

                    // Ejecutamos la consulta y leemos el resultado
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Si encontró una coincidencia, devolvemos la pregunta y su respuesta asociada
                            string preguntaEncontrada = reader.GetString(0);
                            string respuestaEncontrada = reader.GetString(1);
                            return $"{preguntaEncontrada} → {respuestaEncontrada}";
                        }
                        else
                        {
                            // Si no encontró nada, devolvemos un mensaje genérico
                            return "No encontré una respuesta para tu consulta. Reformúlala o contactá RRHH.";
                        }
                    }
                }
            }
        }

        // Este método toma la pregunta y la divide en palabras clave (tokens),
        // usando espacios y convirtiendo todo a minúsculas.
        private string[] ObtenerKeywords(string pregunta)
        {
            return pregunta.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }
    }

    // Clase auxiliar que representa el JSON que espera recibir el endpoint
    public class ConsultaRequest
    {
        public string Pregunta { get; set; }       // Texto de la pregunta del usuario
        public bool EstaLogueado { get; set; }     // Si el usuario está autenticado o no
    }
}



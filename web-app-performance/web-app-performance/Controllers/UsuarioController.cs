using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Newtonsoft.Json;
using StackExchange.Redis;
using web_app_performance.Model;

namespace web_app_performance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private static ConnectionMultiplexer redis;

        [HttpGet]

        public async Task<IActionResult> GetUsuario() 
        {
            string key = "getusuario";
            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyExpireAsync(key, TimeSpan.FromSeconds(10));
            string user = await db.StringGetAsync(key);

            if (!string.IsNullOrEmpty(user))
            {
                return Ok(user);
            }


            string connectionString = "Server = localhost;Database=sys;User=root;Password=123;";
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            string query = "select Id, Nome, Email from usuarios;";
            var usuarios = await connection.QueryAsync <Usuario>(query);

            string usuariosJson  = JsonConvert.SerializeObject(usuarios);
            await db.StringSetAsync(key, usuariosJson);

            return Ok(usuarios);   

        }

        [HttpPost]

        public async Task<IActionResult> Post([FromBody] Usuario usuario)
        {
            string connectionString = "Server = localhost;Database=sys;User=root;Password=123;";
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            

            string sql = @"insert into usuarios(nome, email) values (@nome, @email);";
            await connection.ExecuteAsync(sql, usuario);

            //apagar o cache
            //conexao c redis
            string key = "getusuario";
            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyDeleteAsync(key);

            return Ok();


        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Usuario usuario)
        {
            string connectionString = "Server = localhost;Database=sys;User=root;Password=123;";
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();


            string sql = @"update usuarios set Nome = @nome, Email = @email where Id = @id;";
            await connection.ExecuteAsync(sql, usuario);

            //apagar o cache
            //conexao c redis
            string key = "getusuario";
            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyDeleteAsync(key);

            return Ok();

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string connectionString = "Server = localhost;Database=sys;User=root;Password=123;";
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();


            string sql = @"delete from usuarios where Id = @id;";
            await connection.ExecuteAsync(sql, new {id});

            //apagar o cache
            //conexao c redis
            string key = "getusuario";
            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyDeleteAsync(key);

            return Ok();

        }
    }
}

//docker run --name database-mysql -e MYSQL_ROOT_PASSWORD=123 -p 3306:3306 - d mysql


//docker ps

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
    public class ProdutosController : ControllerBase
    {
        private static ConnectionMultiplexer redis;

        [HttpGet]

        public async Task<IActionResult> GetProdutos()
        {
            string key = "getprodutos";
            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyExpireAsync(key, TimeSpan.FromMinutes(10));
            string produto = await db.StringGetAsync(key);

            if (!string.IsNullOrEmpty(produto))
            {
                return Ok(produto);
            }


            string connectionString = "Server = localhost;Database=sys;User=root;Password=123;";
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            string query = "select id, nome, preco, quant_estoque, data_criacao from produtos;";
            var produtos = await connection.QueryAsync<Produtos>(query);

            string produtosJson = JsonConvert.SerializeObject(produtos);
            await db.StringSetAsync(key, produtosJson);

            return Ok(produtos);

        }

        [HttpPost]

        public async Task<IActionResult> Post([FromBody] Produtos produtos)
        {
            string connectionString = "Server = localhost;Database=sys;User=root;Password=123;";
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();


            string sql = @"insert into produtos(nome, preco, quant_estoque, data_criacao) values (@nome, @preco, @quant_estoque, @data_criacao);";
            await connection.ExecuteAsync(sql, produtos);

            //apagar o cache
            //conexao c redis
            string key = "getprodutos";
            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyDeleteAsync(key);

            return Ok();


        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Produtos produtos)
        {
            string connectionString = "Server = localhost;Database=sys;User=root;Password=123;";
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();


            string sql = @"update produtos set nome = @nome, preco = @preco, quant_estoque = @quant_estoque, data_criacao = @data_criacao where id = @id;";
            await connection.ExecuteAsync(sql, produtos);

            string key = "getprodutos";
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


            string sql = @"delete from produtos where id = @id;";
            await connection.ExecuteAsync(sql, new { id });

            string key = "getprodutos";
            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyDeleteAsync(key);

            return Ok();

        }
    }
}

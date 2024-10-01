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
    public class ProdutoController : ControllerBase
    {
        private ConnectionMultiplexer redis;

        [HttpGet]
        public async Task<IActionResult> GetProduto()
        {
            string key = "getProduto";
            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyExpireAsync(key, TimeSpan.FromSeconds(10));
            string produto = await db.StringGetAsync(key);

            if(!string.IsNullOrEmpty(produto))
                {
                    return Ok(produto);
                }



            string connectionString = "Server=localhost;Database=sys;User=root;Password=123;";
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            string query = "select Id, Nome, Preco, Quantidade, Data from produtos;";
            var produtos = await connection.QueryAsync<Usuario>(query);
            string produtosJson = JsonConvert.SerializeObject(produtos);
            await db.StringSetAsync(key, produtosJson);

            return Ok(produtos);
        } 
        [HttpPost]

        public async Task<IActionResult> PostProduto([FromBody] Produtos produto)
        {
            string connectionString = "Server=localhost;Database=sys;User=root;Password=123;";
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = "INSERT INTO produtos(Nome,Preco,Quantidade,Data) VALUES (@nome,@preco,@quantidade,@data)";
            await connection.ExecuteAsync(sql, produto);

            //apaga o cache
            string key = "getProduto";
            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyDeleteAsync(key);

            return Ok(produto);

        }

        [HttpPut]
        public async Task<IActionResult> PutProduto([FromBody] Produtos produto)
        {
            string connectionString = "Server=localhost;Database=sys;User=root;Password=123";
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = "UPDATE produtos SET Nome = @nome, Preco = @preco WHERE Id = @id";
            await connection.ExecuteAsync(sql, produto);

            //apaga o cache
            string key = "getProduto";
            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyDeleteAsync(key);

            return Ok();

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string connectionString = "Server=localhost;Database=sys;User=root;Password=123";
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string sql = "DELETE FROM produtos WHERE id = @id";
            await connection.ExecuteAsync(sql, new { id });

            //apaga o cache
            string key = "getProduto";
            redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase db = redis.GetDatabase();
            await db.KeyDeleteAsync(key);

            return Ok();

        }
    }
}

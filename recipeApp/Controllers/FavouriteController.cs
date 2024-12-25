using Microsoft.AspNetCore.Mvc;
using recipeApp.Model;
using System.Data;
using System.Data.SqlClient;

namespace recipeApp.Controllers
{
    [Route("api/Favourite")]
    [ApiController]
    public class FavouriteController : ControllerBase
    {
        private readonly AppDbContext _context;
        private string _connectionString;

        public FavouriteController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        // POST: api/Favourite/postFavourite
        [HttpPost("postFavourite")]
        public async Task<IActionResult> PostFavourite([FromBody] FavouriteModel request)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string query;
                int userId;

                // Kullanıcı adından UserId'yi bulma
                string userQuery = "SELECT ID FROM USERS WHERE USER_NAME = @UserName";
                var command = new SqlCommand(userQuery, connection);
                command.Parameters.AddWithValue("@UserName", request.UserName); // UserName parametresi

                await connection.OpenAsync();

                var userIdResult = await command.ExecuteScalarAsync();
                if (userIdResult == null)
                {
                    return BadRequest("User not found");
                }

                userId = Convert.ToInt32(userIdResult);

                // Yemek adı üzerinden işlem yapacağız
                if (request.IsFavourite)
                {
                    // Favorilere ekle
                    query = @"
                    INSERT INTO FAVOURITE_TABLE (FAVOURITE_NAME, CREATED_AT, USER_ID)
                    VALUES (@FoodName, GETDATE(), @UserId)";
                }
                else
                {
                    // Favorilerden çıkar
                    query = @"
                    DELETE FROM FAVOURITE_TABLE
                    WHERE USER_ID = @UserId AND FAVOURITE_NAME = @FoodName";
                }

                command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@FoodName", request.FavouriteName); // FAVOURITE_NAME parametresi

                var affectedRows = await command.ExecuteNonQueryAsync();

                if (affectedRows > 0)
                {
                    return Ok(new { success = true });
                }
                else
                {
                    return BadRequest("Operation failed");
                }
            }
        }
        [HttpGet("getFoodsAndRecipes")]
        public async Task<IActionResult> GetFoodsAndRecipes(string userName)
        {
            var foodsAndRecipes = new List<object>();

            using (var connection = new SqlConnection(_connectionString))
            {
                // SQL Sorgusu: yemek adları ve tarif adları
                string query = @"
        SELECT 
            F.FOOD_NAME AS FoodName,
            R.RECIPE_NAME AS RecipeName,
            CASE WHEN FT.FAVOURITE_NAME IS NOT NULL THEN 1 ELSE 0 END AS IsFavourite
        FROM FOODS_TABLE F
        LEFT JOIN FAVOURITE_TABLE FT 
            ON F.FOOD_NAME = FT.FAVOURITE_NAME
        LEFT JOIN RECIPE_TABLE R 
            ON F.ID = R.FOOD_ID
        WHERE FT.USER_ID = (SELECT ID FROM USERS WHERE USER_NAME = @UserName)";

                try
                {
                    var command = new SqlCommand(query, connection);
                    command.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = userName;

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            foodsAndRecipes.Add(new
                            {
                                foodName = reader["FoodName"].ToString(),
                                recipeName = reader["RecipeName"].ToString(),
                                IsFavourite = reader.GetInt32(2) == 1 // Eğer 1 ise, o yemek favori
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Hata yönetimi
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Ok(foodsAndRecipes);
        }


    }
}

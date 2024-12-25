using Microsoft.AspNetCore.Mvc;
using recipeApp.Model;
using System.Data.SqlClient;
using System.Web.Providers.Entities;

namespace recipeApp.Controllers
{
    [Route("api/recipes")]
    [ApiController]
    public class RecipeController : Controller
    {
        private readonly AppDbContext _context;
        private string _connectionString;

        public RecipeController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Index metodu (isteğe bağlı)
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Recipe API is running.");
        }

        // Kullanıcı ID'sini kullanıcı adına göre al
        [HttpGet("getUserRecipe")]
        public async Task<IActionResult> GetUserIdByUsername(string username)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"SELECT ID FROM USERS WHERE USER_NAME = @Username";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);

                connection.Open();
                var userId = await command.ExecuteScalarAsync();
                connection.Close();

                if (userId != null)
                {
                    return Ok(new { UserID = (int)userId });
                }
                else
                {
                    return NotFound("User not found.");
                }
            }
        }

        // Kategori ID'sini kategori adına göre al
        [HttpGet("getCategoryIdRecipe")]
        public async Task<IActionResult> GetCategoryIdByName(string categoryName)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"SELECT ID FROM CATEGORY_TABLE WHERE CATEGORY_NAME = @CategoryName";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CategoryName", categoryName);

                connection.Open();
                var categoryId = await command.ExecuteScalarAsync();
                connection.Close();

                if (categoryId != null)
                {
                    return Ok(new { CategoryId = (int)categoryId });
                }
                else
                {
                    return NotFound("Category not found.");
                }
            }
        }

        // Tarif ekle
        [HttpPost("addRecipe")]
        public async Task<IActionResult> AddRecipe([FromBody] RecipeModel request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.CategoryName))
            {
                return BadRequest("Invalid data.");
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    // Kullanıcı ID'sini al
                    string userIdQuery = "SELECT ID FROM USERS WHERE USER_NAME = @Username";
                    SqlCommand userIdCommand = new SqlCommand(userIdQuery, connection);
                    userIdCommand.Parameters.AddWithValue("@Username", request.Username);

                    connection.Open();
                    var userIdResult = await userIdCommand.ExecuteScalarAsync();
                    if (userIdResult == null)
                    {
                        return NotFound("User not found.");
                    }
                    int userId = (int)userIdResult;

                    // Kategori ID'sini al
                    string categoryIdQuery = "SELECT ID FROM CATEGORY_TABLE WHERE CATEGORY_NAME = @CategoryName";
                    SqlCommand categoryIdCommand = new SqlCommand(categoryIdQuery, connection);
                    categoryIdCommand.Parameters.AddWithValue("@CategoryName", request.CategoryName);

                    var categoryIdResult = await categoryIdCommand.ExecuteScalarAsync();
                    if (categoryIdResult == null)
                    {
                        return NotFound("Category not found.");
                    }
                    int categoryId = (int)categoryIdResult;

                    // Tarif ekle
                    string insertRecipeQuery = @"
                        INSERT INTO FOODS_TABLE (CATEGORY_ID, FOOD_NAME, CREATED_AT, USER_ID)
                        VALUES (@CategoryID, @FoodName, GETDATE(), @UserID);";

                    SqlCommand insertCommand = new SqlCommand(insertRecipeQuery, connection);
                    insertCommand.Parameters.AddWithValue("@CategoryID", categoryId);
                    insertCommand.Parameters.AddWithValue("@FoodName", request.FoodName);
                    insertCommand.Parameters.AddWithValue("@UserID", userId);

                    int result = await insertCommand.ExecuteNonQueryAsync();
                    connection.Close();

                    if (result > 0)
                    {
                        return Ok("Recipe added successfully.");
                    }
                    else
                    {
                        return StatusCode(500, "An error occurred while adding the recipe.");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }

        [HttpPost("addCookingTime")]
        public async Task<IActionResult> AddCookingTime([FromBody] CookingModel request)
        {
            if (request == null || string.IsNullOrEmpty(request.FoodName))
            {
                return BadRequest("Invalid data.");
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    // Kullanıcı ID'sini al
                    string foodIdQuery = "SELECT ID FROM FOODS_TABLE WHERE FOOD_NAME = @FoodName";
                    SqlCommand foodIdCommand = new SqlCommand(foodIdQuery, connection);
                    foodIdCommand.Parameters.AddWithValue("@FoodName", request.FoodName);

                    connection.Open();
                    var foodIdResult = await foodIdCommand.ExecuteScalarAsync();
                    if (foodIdResult == null)
                    {
                        return NotFound("Food not found.");
                    }
                    int foodId = (int)foodIdResult;


                    // Tarif ekle
                    string insertRecipeQuery = @"
                        INSERT INTO COOKING_TIME_TABLE (FOOD_ID, PREP_TIME, COOK_TIME)
                        VALUES (@FoodId, @PrepTime, @CookTime);";

                    SqlCommand insertCommand = new SqlCommand(insertRecipeQuery, connection);
                    insertCommand.Parameters.AddWithValue("@FoodId", foodId);
                    insertCommand.Parameters.AddWithValue("@PrepTime", request.PrepTime);
                    insertCommand.Parameters.AddWithValue("@CookTime", request.CookTime);

                    int result = await insertCommand.ExecuteNonQueryAsync();
                    connection.Close();

                    if (result > 0)
                    {
                        return Ok("Recipe added successfully.");
                    }
                    else
                    {
                        return StatusCode(500, "An error occurred while adding the recipe.");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }
        [HttpPost("addRecipeTable")]
        public async Task<IActionResult> AddRecipeTable(
            [FromBody] RecipeTableModel request,
            [FromQuery] string username,
            [FromQuery] string categoryName)
        {
            if (request == null || string.IsNullOrEmpty(request.FoodName) || string.IsNullOrEmpty(request.RecipeName))
            {
                return BadRequest("Invalid data. Both FoodName and RecipeName are required.");
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(categoryName))
            {
                return BadRequest("Username and CategoryName are required.");
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    // Yemeği kontrol et
                    string foodCheckQuery = "SELECT ID FROM FOODS_TABLE WHERE FOOD_NAME = @FoodName";
                    SqlCommand foodCheckCommand = new SqlCommand(foodCheckQuery, connection);
                    foodCheckCommand.Parameters.AddWithValue("@FoodName", request.FoodName);

                    object foodIdResult = await foodCheckCommand.ExecuteScalarAsync();
                    int foodId;

                    // Yemek bulunmazsa, AddRecipe API'sini çağır
                    if (foodIdResult == null)
                    {
                        // AddRecipe metodunda username ve categoryName yerine request parametrelerini kullan
                        var addRecipeResponse = await AddRecipe(new RecipeModel
                        {
                            FoodName = request.FoodName,
                            Username = username,  // user yerine username
                            CategoryName = categoryName  // request.CategoryName yerine categoryName
                        });

                        if (addRecipeResponse is OkObjectResult)
                        {
                            // Yeni yemeğin ID'sini tekrar al
                            foodIdResult = await foodCheckCommand.ExecuteScalarAsync();
                            if (foodIdResult == null)
                            {
                                return StatusCode(500, "Failed to retrieve newly added food.");
                            }
                            foodId = (int)foodIdResult;
                        }
                        else
                        {
                            return addRecipeResponse;
                        }
                    }
                    else
                    {
                        foodId = (int)foodIdResult;
                    }

                    // Tarif ekle
                    string insertRecipeQuery = @"
                    INSERT INTO RECIPE_TABLE (RECIPE_NAME, CREATED_AT, FOOD_ID)
                    VALUES (@RecipeName, GETDATE(), @FoodId);";

                    SqlCommand insertRecipeCommand = new SqlCommand(insertRecipeQuery, connection);
                    insertRecipeCommand.Parameters.AddWithValue("@RecipeName", request.RecipeName);
                    insertRecipeCommand.Parameters.AddWithValue("@FoodId", foodId);

                    int result = await insertRecipeCommand.ExecuteNonQueryAsync();
                    connection.Close();

                    if (result > 0)
                    {
                        return Ok("Recipe added successfully.");
                    }
                    else
                    {
                        return StatusCode(500, "An error occurred while adding the recipe.");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }

        [HttpGet("getRecipes")]
        public async Task<IActionResult> GetRecipes()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;

                    // Tüm food_id'lere karşılık gelen food_name ve recipe_name'i alıyoruz
                    command.CommandText = @"
                SELECT f.FOOD_NAME, r.RECIPE_NAME
                FROM RECIPE_TABLE r
                INNER JOIN FOODS_TABLE f
                ON r.FOOD_ID = f.ID";

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        var recipes = new List<object>();
                        while (await reader.ReadAsync())
                        {
                            var recipe = new
                            {
                                title = reader["FOOD_NAME"].ToString(),  // FOOD_NAME başlık olarak
                                description = reader["RECIPE_NAME"].ToString()  // RECIPE_NAME açıklama olarak
                            };
                            recipes.Add(recipe);
                        }

                        // Veriyi kontrol et
                        if (recipes.Count > 0)
                        {
                            return Ok(new { recipes });
                        }
                        else
                        {
                            return NotFound("No recipes found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


    }
}

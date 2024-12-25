using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using recipeApp.Model;
using System.Data.SqlClient;

[Route("api/[controller]")]
[ApiController]
public class FoodsController : ControllerBase
{
    private readonly AppDbContext _context;
    private string _connectionString;

    public FoodsController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // GET: api/food
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FoodModel>>> GetFoods()
    {
        // Veritabanındaki yemekleri listele
        var foods = await _context.Foods
            .Select(f => new FoodModel { Id = f.Id, Name = f.Name }) // FoodModel'e dönüştürme
            .ToListAsync();
        return Ok(foods);
    }

    // GET: api/food/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<FoodModel>> GetFood(int id)  // Kişiye özel
    {
        // Belirli bir yemek ID ile alınır
        var food = await _context.Foods
            .Where(f => f.Id == id)
            .Select(f => new FoodModel { Id = f.Id, Name = f.Name })
            .FirstOrDefaultAsync();

        if (food == null)
        {
            return NotFound();
        }

        return Ok(food);
    }
    [HttpPost]
    public IActionResult AddFood([FromBody] FoodModel food)
    {
        if (food == null || string.IsNullOrEmpty(food.Name))
        {
            return BadRequest("Geçersiz veri.");
        }

        // SQL komutu ile en son yemek ID'sini almak
        // Son yemek ID'sini almak için SQL sorgusunu oluştur
        string getLastIdQuery = "SELECT TOP 1 FOOD_ID FROM Foods ORDER BY FOOD_ID DESC";

        int nextId = 1; // İlk yemekse ID 1 olacak

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            try
            {
                connection.Open();  // Bağlantıyı açıyoruz

                // Son yemek ID'sini alıyoruz
                SqlCommand command = new SqlCommand(getLastIdQuery, connection);
                var result = command.ExecuteScalar(); // Son ID'yi alıyoruz

                if (result != DBNull.Value) // Eğer yemek varsa
                {
                    nextId = Convert.ToInt32(result) + 1; // Son ID'yi alıp bir artırıyoruz
                }

                // Yemek eklemek için INSERT komutunu hazırlıyoruz
                string insertQuery = "INSERT INTO FOODS (FOOD_ID, FOOD_NAME) VALUES (@FoodId, @FoodName)";
                SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@FoodId", nextId);
                insertCommand.Parameters.AddWithValue("@FoodName", food.Name.Trim()); // Parametreyi doğru şekilde ekleyin

                // Insert komutunu çalıştırıyoruz
                insertCommand.ExecuteNonQuery();

                return Ok(new { id = nextId, name = food.Name }); // Başarıyla eklenen veriyi döndürüyoruz
            }
            catch (Exception ex)
            {
                // Hata durumunda daha fazla bilgi sağlayabiliriz
                return StatusCode(500, $"İç sunucu hatası: {ex.Message}");
            }
        }

    }
    [HttpGet("getCategories")]
    public IActionResult GetCategories()
    {
        var categories = new List<string>();

        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // SQL sorgusu
                string sqlQuery = "SELECT CATEGORY_NAME FROM CATEGORY_TABLE";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(reader["CATEGORY_NAME"].ToString());
                        }
                    }
                }
            }

            return Ok(categories);
        }
        catch (SqlException ex)
        {
            return StatusCode(500, $"Veritabanı hatası: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Beklenmeyen bir hata oluştu: {ex.Message}");
        }
    }
}


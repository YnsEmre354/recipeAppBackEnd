using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using recipeApp.Model;
using System.Data;
using System.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private String _connectionString;

    public UsersController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // GET: api/Users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserModel>>> GetUsers()
    {
        var users = await _context.Users
            .Select(x => new UserModel{ Id = x.Id, Name = x.Name, Email = x.Email, Password = x.Password })
            .ToListAsync();
        return Ok(users);
    }


    [HttpPost("login")]
    public async Task<ActionResult<UserModel>> Login([FromBody] LoginModel login)
    {
        try
        {
            var user = await _context.Users
                                      .FromSqlInterpolated($"SELECT * FROM USERS WHERE USER_NAME = {login.Name} AND USER_PASSWORD = {login.Password}")
                                      .FirstOrDefaultAsync();

            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }

            return Ok(new UserModel { Name = user.Name , Password = user.Password, Email = user.Email, Id = user.Id});
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] PasswordRequestModel request)
    {
        // SQL sorgusunu hazırlıyoruz
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = "UPDATE USERS SET USER_PASSWORD = @NewPassword WHERE USER_NAME = @Username";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Username", request.UserName);
                command.Parameters.AddWithValue("@NewPassword", request.Password);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    return Ok(new { message = "Password reset successfully!" });
                }
                else
                {
                    return NotFound(new { message = "User not found!" });
                }
            }
        }
    }

    [HttpPost("isUserName")]
    public async Task<ActionResult<UserModel>> isUserName([FromBody] UserModel userModel)
    {
        try
        {
            var user = await _context.Users
                                      .FromSqlInterpolated($"SELECT * FROM USERS WHERE USER_NAME = {userModel.Name}")
                                      .FirstOrDefaultAsync();

            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }

            return Ok(new UserModel { Name = user.Name, Password = user.Password, Email = user.Email, Id = user.Id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("addUser")]
    public async Task<ActionResult<UserModel>> insertUser([FromBody] UserModel userModel)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "INSERT INTO USERS (USER_NAME, USER_EMAIL, USER_PASSWORD) VALUES (@UserName, @Email, @Password)";

                using (var command = new SqlCommand(query, connection))
                {
                    // Parametreleri eklerken boyut belirtiyoruz
                    command.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 250)).Value = userModel.Name;
                    command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 250)).Value = userModel.Email;
                    command.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar, 50)).Value = userModel.Password;

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok(new { message = "User successfully added!" });
                    }
                    else
                    {
                        return Unauthorized("Failed to add user.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }



    // GET: api/Users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<UserModel>> GetUser(int id)  //Kişiye özel
    {
        var users = await _context.Users
            .Where(x => x.Id == id)
            .Select(x => new UserModel { Id = x.Id, Name = x.Name, Email = x.Email, Password = x.Password })
            .FirstOrDefaultAsync();

        if (users == null)
        {
            return NotFound();
        }

        return Ok(users);
    }

    // POST: api/Users
    [HttpPost]
    public async Task<ActionResult<UserModel>> PostUser(UserModel user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetUser", new { id = user.Id }, user);
    }

    // PUT: api/Users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(int id, UserModel user)
    {
        if (id != user.Id)
        {
            return BadRequest();
        }

        _context.Entry(user).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}

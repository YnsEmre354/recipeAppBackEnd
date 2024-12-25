using Microsoft.EntityFrameworkCore;

namespace recipeApp.Model
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }

        public DbSet<FoodModel> Foods { get; set; }

    }
}

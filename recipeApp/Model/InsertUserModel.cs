using System.ComponentModel.DataAnnotations.Schema;

namespace recipeApp.Model
{
    public class InsertUserModel
    {
        [Column("USER_NAME")]
        public string? Name { get; set; }

        [Column("USER_EMAIL")]
        public string? Email { get; set; }

        [Column("USER_PASSWORD")]
        public string? Password { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace recipeApp.Model
{
    public class UserModel
    {
        [Column("ID")]
        public int? Id { get; set; }

        [Column("USER_NAME")]
        public string? Name { get; set; }

        [Column("USER_EMAIL")]
        public string? Email { get; set; }

        [Column("USER_PASSWORD")]
        public string? Password { get; set; }
    }
}

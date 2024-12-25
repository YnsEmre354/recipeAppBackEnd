using System.ComponentModel.DataAnnotations.Schema;

namespace recipeApp.Model
{
    public class FoodModel
    {
        [Column("FOOD_ID")]
        public int? Id { get; set; }

        [Column("FOOD_NAME")]
        public string? Name { get; set; }

    }
}

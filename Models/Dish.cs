namespace RPM.Models
{
	public class Dish
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Category { get; set; }
		public decimal Price { get; set; }
		public bool IsAvailable { get; set; }
		public string Description { get; set; }
	}
}

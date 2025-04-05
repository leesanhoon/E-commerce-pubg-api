namespace E_commerce_pubg_api.Application.DTOs
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CreateCategoryDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class UpdateCategoryDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
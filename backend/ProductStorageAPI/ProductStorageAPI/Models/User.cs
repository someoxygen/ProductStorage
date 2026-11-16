namespace ProductStorageAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; } = true;
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime UpdateDate { get; set; }
    }
}

namespace ProductStorageAPI.Models
{
    public class TokenModel
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}

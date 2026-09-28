namespace FleetFlow.Api.Models;

public class RefreshToken
{
    public int RefreshTokenId { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}


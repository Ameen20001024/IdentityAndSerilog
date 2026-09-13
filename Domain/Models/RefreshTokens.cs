namespace IdentityAndSerilog.Domain.Models;

public class RefreshTokens
{
    public int Id { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public string AccessTokenHash { get; set; } = string.Empty;

    public int UserId { get; set; } 

    public DateTime CreatedAt { get; set; }

    public DateTime AccessTokenExpiresAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public User User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public bool IsRevoked => RevokedAt.HasValue;

    public bool IsActive => !IsExpired && !IsRevoked;
}
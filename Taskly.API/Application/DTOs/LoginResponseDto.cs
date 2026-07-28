public class LoginResponseDto
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public UserResponseDto User { get; set; } = null!;
}
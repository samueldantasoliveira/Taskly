public class UserResponseDto
{
    public long Version { get; set; }
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}

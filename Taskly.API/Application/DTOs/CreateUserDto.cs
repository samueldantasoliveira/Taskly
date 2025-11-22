namespace Taskly.Application.DTOs
{

    public class CreateUserDto
    {
        public required string Name { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
    }
}
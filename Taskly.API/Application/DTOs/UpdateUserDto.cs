using System.ComponentModel.DataAnnotations;
using Taskly.Domain;

namespace Taskly.Application.DTOs
{
    public class UpdateUserDto
    {
        public string? Name { get; set;}
        public string? Email {get; set;}
        public string? Password { get; set;}
    }
}
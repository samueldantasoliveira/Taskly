using System.ComponentModel.DataAnnotations;
using Rivulus.Domain;

namespace Rivulus.Application.DTOs
{
public class UpdateUserDto
    {
        public long? Version { get; set; }
        public string? Name { get; set;}
        public string? Email {get; set;}
        public string? Password { get; set;}
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Taskly.Application.DTOs;

public class CreateUserDto
{
    [Required]
    public required string Name {get; set;}
}

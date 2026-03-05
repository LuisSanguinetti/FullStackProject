using System.ComponentModel.DataAnnotations;

namespace obligatorio.WebApi.DTO;

public class UserUpdateDto
{
    public string? Name { get; init; }

    public string? Surname { get; init; }

    public string? Email { get; init; }

    public string? Password { get; init; }
   public DateOnly? DateOfBirth { get; init; }
}

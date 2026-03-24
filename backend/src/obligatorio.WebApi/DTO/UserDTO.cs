using System.ComponentModel.DataAnnotations;

namespace obligatorio.WebApi.DTO;

public class UserCreateBaseDto
{
    [Required, MinLength(2)]
    public string Name { get; set; } = string.Empty;
    [Required, MinLength(2)]
    public string Surname { get; set; } = string.Empty;
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required, MinLength(1)]
    public string Password { get; set; } = string.Empty;
    [Required]
    public DateOnly DateOfBirth { get; set; }
}

public class AdminCreateDto : UserCreateBaseDto { }
public class OperatorCreateDto : UserCreateBaseDto { }

public class VisitorCreateDto : UserCreateBaseDto
{
}

public class UserGetDtos
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public UserGetDtos(Guid id, string name, string surname, string email, string role)
    {
        Id = id;
        Name = name;
        Surname = surname;
        Email = email;
        Role = role;
    }
}

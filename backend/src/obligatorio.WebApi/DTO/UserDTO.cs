using System;
using System.ComponentModel.DataAnnotations;
using Domain;

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
    [Required]
    public MembershipLevel Membership { get; set; }
}

public class UserGetDtos
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public MembershipLevel Membership { get; set; }

    public UserGetDtos(Guid id, string name, string surname, string email, string role, MembershipLevel membership)
    {
        Id = id;
        Name = name;
        Surname = surname;
        Email = email;
        Role = role;
        Membership = membership;
    }
}

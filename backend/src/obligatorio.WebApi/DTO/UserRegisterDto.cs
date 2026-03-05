using System.ComponentModel.DataAnnotations;
using Domain;

namespace obligatorio.WebApi.DTO;

public sealed class UserRegisterDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(80, ErrorMessage = "Name can’t exceed 80 characters")]
    public string Name { get; init; } = default!;

    [Required(ErrorMessage = "Surname is required")]
    [StringLength(80, ErrorMessage = "Surname can’t exceed 80 characters")]
    public string Surname { get; init; } = default!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email format is invalid")]
    public string Email { get; init; } = default!;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; init; } = default!;

    [Required(ErrorMessage = "Date of birth is required")]
    public DateOnly DateOfBirth { get; init; }

    public static User DtoToDom(UserRegisterDto d) =>
        new User(d.Name, d.Surname, d.Email, d.Password, d.DateOfBirth, MembershipLevel.Standard);
}

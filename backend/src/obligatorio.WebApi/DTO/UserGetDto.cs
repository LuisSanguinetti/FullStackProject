using Domain;

namespace obligatorio.WebApi.DTO;

public class UserGetDto
{
    public Guid Id { get; set; }
    public string Name { get; init; } = default!;
    public string Surname { get; init; } = default!;
    public string Email { get; init; } = default!;

    public static UserGetDto DomToDto(User u) => new UserGetDto
    {
        Id = u.Id,
        Name = u.Name,
        Surname = u.Surname,
        Email = u.Email
    };
}

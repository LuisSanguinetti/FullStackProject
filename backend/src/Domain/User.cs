using System.Diagnostics.CodeAnalysis;
using Guid = System.Guid;

namespace Domain;

public class User
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string Surname { get; set; } = string.Empty;
    public required string Email { get; set; }
    public required string Password { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public int Points { get; set; }

    public User()
    {
    }

    [SetsRequiredMembers]
    public User(string name, string surname, string email, string password, DateOnly dateOfBirth)
    {
        Id = Guid.NewGuid();
        Name = name;
        Surname = surname;
        Email = email;
        Password = password;
        DateOfBirth = dateOfBirth;
        Points = 0;
    }
}

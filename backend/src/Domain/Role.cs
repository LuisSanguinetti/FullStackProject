namespace Domain;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public Role()
    {
    }

    public Role(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }
}

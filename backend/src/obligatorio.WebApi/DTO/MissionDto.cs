namespace obligatorio.WebApi.DTO;

public class MissionDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int BasePoints { get; set; }
}

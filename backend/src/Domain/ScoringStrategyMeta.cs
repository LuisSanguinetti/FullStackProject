using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class ScoringStrategyMeta
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public bool IsDeleted { get; set; } = false;
    public required string FilePath { get; set; }
    public required string FileName { get; set; }

    public ScoringStrategyMeta()
    {
    }

    [SetsRequiredMembers]
    public ScoringStrategyMeta(string name, DateTime createdOn, string filePath, string fileName)
    {
        Id = Guid.NewGuid();
        Name = name;
        CreatedOn = createdOn;
        FilePath = filePath;
        FileName = fileName;
    }
}

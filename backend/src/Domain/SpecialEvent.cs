using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class SpecialEvent
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public int Capacity { get; set; }
    public decimal ExtraPrice { get; set; }
    public List<Attraction> Attractions { get; set; } = [];

    public SpecialEvent()
    {
    }

    [SetsRequiredMembers]
    public SpecialEvent(string name, DateTime start, DateTime end, int capacity, decimal extraPrice, List<Attraction>? attractions)
    {
        Id = Guid.NewGuid();
        Name = name;
        StartDate = start;
        EndDate = end;
        Capacity = capacity;
        ExtraPrice = extraPrice;
        if(attractions != null)
        {
            Attractions.AddRange(attractions);
        }
    }
}

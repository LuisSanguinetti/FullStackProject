using System;
using System.Collections.Generic;
using Domain;

namespace IParkBusinessLogic;

public interface ISpecialEventAdminLogic
{
    SpecialEvent Create(string name, DateTime start, DateTime end, int capacity, decimal extraPrice, IEnumerable<Guid> attractionIds);
    void Delete(Guid id);
    IEnumerable<SpecialEvent> List();
}

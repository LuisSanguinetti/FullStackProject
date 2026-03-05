using System;
using System.Collections.Generic;
using Domain;

namespace IParkBusinessLogic;

public interface IAttractionAdminLogic
{
    Attraction Create(string name, AttractionType type, int minAge, int capacity, string description, int basePoints = 0);
    Attraction Update(Guid id, string name, AttractionType type, int minAge, int capacity, string description, int basePoints = 0, bool? enabled = null);
    void Delete(Guid id);
    IEnumerable<Attraction> List(AttractionType? type = null, bool? enabled = null);
    void SetEnabled(Guid id, bool enabled);
    Attraction GetOrThrow(Guid id);
}

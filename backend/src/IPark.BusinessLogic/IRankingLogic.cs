using System;
using System.Collections.Generic;

namespace IParkBusinessLogic;

public record DailyRankingEntry(
    Guid UserId,
    string Name,
    string Surname,
    string Email,
    int TotalPoints
);

public interface IRankingLogic
{
    IEnumerable<DailyRankingEntry> GetDailyTop(int limit = 10);
}

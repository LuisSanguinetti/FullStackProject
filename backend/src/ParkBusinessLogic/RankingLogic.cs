using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Domain;
using IDataAccess;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public class RankingLogic : IRankingLogic
{
    private readonly IRepository<PointsAward> _pointsRepo;
    private readonly IScoringStrategyQueryLogic _strategy;
    private readonly ISystemClock _clock;

    public RankingLogic(
        IRepository<PointsAward> pointsRepo,
        IScoringStrategyQueryLogic strategy,
        ISystemClock clock)
    {
        _pointsRepo = pointsRepo;
        _strategy = strategy;
        _clock = clock;
    }

    public IEnumerable<DailyRankingEntry> GetDailyTop(int limit = 10)
    {
        if (limit <= 0)
        {
            limit = 10;
        }

        var nowUtc   = _clock.Now();
        var dayStart = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, 0, 0, 0, DateTimeKind.Utc);
        var dayEnd   = dayStart.AddDays(1);

        Expression<Func<PointsAward, bool>> pred =
            p => p.At >= dayStart && p.At < dayEnd;

        // Includes User to avoid get by id
        var includes = new Expression<Func<PointsAward, object>>[] { p => p.User };

        var awards = _pointsRepo.FindAll(pred, includes);

        var top = awards
            .GroupBy(p => new { p.UserId, p.User.Name, p.User.Surname, p.User.Email })
            .Select(g => new DailyRankingEntry(
                g.Key.UserId,
                g.Key.Name,
                g.Key.Surname,
                g.Key.Email,
                g.Sum(x => x.Points)))
            .OrderByDescending(e => e.TotalPoints)
            .ThenBy(e => e.UserId)
            .Take(limit)
            .ToList();

        return top;
    }
}

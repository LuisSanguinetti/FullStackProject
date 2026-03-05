using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Domain;
using FluentAssertions;
using IDataAccess;
using IParkBusinessLogic;
using Moq;

namespace Park.BusinessLogic.Test;

[TestClass]
public class RankingLogicTest
{
    private Mock<IRepository<PointsAward>> _pointsRepo = null!;
    private Mock<IScoringStrategyQueryLogic> _strategy = null!;
    private Mock<ISystemClock> _clock = null!;
    private IRankingLogic _logic = null!;

    [TestInitialize]
    public void Setup()
    {
        _pointsRepo = new Mock<IRepository<PointsAward>>(MockBehavior.Strict);
        _strategy = new Mock<IScoringStrategyQueryLogic>(MockBehavior.Strict);
        _clock = new Mock<ISystemClock>(MockBehavior.Strict);

        _logic = new Park.BusinessLogic.RankingLogic(_pointsRepo.Object, _strategy.Object, _clock.Object);
    }

    [TestMethod]
    public void GetDailyTop_Returns_Top10_ByPoints_Today_ActiveStrategy()
    {
        // arrange
        var now = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        _clock.Setup(c => c.Now()).Returns(now);

        var strat = new ScoringStrategyMeta { Id = Guid.NewGuid(), Name = "Default", IsActive = true, IsDeleted = false, FilePath = "test/path", FileName = "test.dll" };
        _strategy.Setup(s => s.GetActiveOrThrow()).Returns(strat);

        var dayStart = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);

        var awards = new List<PointsAward>();

        for (var i = 0; i < 12; i++)
        {
            var u = new User("user", "x", $"u{i}@x", "p", new DateOnly(1990,1,1), MembershipLevel.Standard);
            awards.Add(new PointsAward
            {
                Id = Guid.NewGuid(),
                UserId = u.Id,
                User = u,
                StrategyId = strat.Id,
                Points = 100 - (i * 5),
                At = now,
                Reason = "Test reason"
            });
        }

        var uOld = new User("user", "x", "old@x", "p", new DateOnly(1990,1,1), MembershipLevel.Standard);
        awards.Add(new PointsAward
        {
            Id = Guid.NewGuid(),
            UserId = uOld.Id,
            User = uOld,
            StrategyId = strat.Id,
            Points = 999,
            At = dayStart.AddDays(-1),
            Reason = "Test reason"
        });

        awards.Add(new PointsAward
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            StrategyId = strat.Id,
            Points = 999,
            At = dayStart.AddDays(-1),
            User = null!,
            Reason = "Test reason"
        });

        _pointsRepo
            .Setup(r => r.FindAll(
                It.IsAny<Expression<Func<PointsAward, bool>>>(),
                It.IsAny<Expression<Func<PointsAward, object>>[]>()))
            .Returns<Expression<Func<PointsAward, bool>>, Expression<Func<PointsAward, object>>[]>((pred, _) =>
                awards.Where(pred.Compile()).ToList());

        // act
        var result = _logic.GetDailyTop();

        // assert
        result.Should().HaveCount(10);
        result.First().TotalPoints.Should().Be(100);
        result.Last().TotalPoints.Should().Be(55);
        result.All(x => x.Name == "user").Should().BeTrue();

        _clock.VerifyAll();
        _pointsRepo.VerifyAll();
    }

    [TestMethod]
    public void GetDailyTop_Empty_Returns_Empty()
    {
        var now = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        _clock.Setup(c => c.Now()).Returns(now);

        var strat = new ScoringStrategyMeta
        {
            Id = Guid.NewGuid(),
            Name = "Default",
            IsActive = true,
            IsDeleted = false,
            FilePath = "test/path",
            FileName = "test.dll"
        };
        _strategy.Setup(s => s.GetActiveOrThrow()).Returns(strat);

        _pointsRepo
            .Setup(r => r.FindAll(
                It.IsAny<Expression<Func<PointsAward, bool>>>(),
                It.IsAny<Expression<Func<PointsAward, object>>[]>()))
            .Returns(new List<PointsAward>());

        var result = _logic.GetDailyTop();
        result.Should().BeEmpty();

        _clock.VerifyAll();
        _pointsRepo.Verify(r => r.FindAll(
            It.IsAny<Expression<Func<PointsAward, bool>>>(),
            It.IsAny<Expression<Func<PointsAward, object>>[]>()), Times.Once);
        _pointsRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetDailyTop_Aggregates_MultipleAwards_Per_User()
    {
        var now = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        _clock.Setup(c => c.Now()).Returns(now);

        var strat = new ScoringStrategyMeta
        {
            Id = Guid.NewGuid(),
            Name = "Default",
            IsActive = true,
            IsDeleted = false,
            FilePath = "test/path",
            FileName = "test.dll"
        };
        _strategy.Setup(s => s.GetActiveOrThrow()).Returns(strat);

        var user = new User("user", "x", "u@x", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard);

        var awards = new List<PointsAward>
        {
            new PointsAward
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                StrategyId = strat.Id,
                Points = 40,
                At = now,
                Reason = "Test reason"
            },
            new PointsAward
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                StrategyId = strat.Id,
                Points = 60,
                At = now,
                Reason = "Test2"
            }
        };

        _pointsRepo
            .Setup(r => r.FindAll(
                It.IsAny<Expression<Func<PointsAward, bool>>>(),
                It.IsAny<Expression<Func<PointsAward, object>>[]>()))
            .Returns<Expression<Func<PointsAward, bool>>, Expression<Func<PointsAward, object>>[]>((pred, _) =>
                awards.Where(pred.Compile()).ToList());

        var result = _logic.GetDailyTop(10).ToList();

        result.Should().ContainSingle();
        result[0].UserId.Should().Be(user.Id);
        result[0].TotalPoints.Should().Be(100);
        result[0].Name.Should().Be("user");
    }

    [TestMethod]
    public void GetDailyTop_Filters_By_Today_Only()
    {
        var now = new DateTime(2025, 2, 1, 12, 0, 0, DateTimeKind.Utc);
        _clock.Setup(c => c.Now()).Returns(now);

        var strat = new ScoringStrategyMeta { Id = Guid.NewGuid(), Name = "Default", IsActive = true, IsDeleted = false, FilePath = "test/path", FileName = "test.dll" };
        _strategy.Setup(s => s.GetActiveOrThrow()).Returns(strat);

        var uYesterday = new User("Yesterday","X","y@x","p", new DateOnly(1990,1,1), MembershipLevel.Standard);
        var uToday     = new User("Today","X","t@x","p", new DateOnly(1990,1,1), MembershipLevel.Standard);

        var awards = new List<PointsAward>
        {
            new PointsAward
            {
                Id = Guid.NewGuid(),
                UserId = uYesterday.Id,
                User = uYesterday,
                StrategyId = strat.Id,
                Points = 10,
                At = now.AddDays(-1),
                Reason = "Test reason"
            },
            new PointsAward
            {
                Id = Guid.NewGuid(),
                UserId = uToday.Id,
                User = uToday,
                StrategyId = strat.Id,
                Points = 20,
                At = now, // hoy
                Reason = "Test2"
            }
        };

        _pointsRepo
            .Setup(r => r.FindAll(
                It.IsAny<Expression<Func<PointsAward, bool>>>(),
                It.IsAny<Expression<Func<PointsAward, object>>[]>()))
            .Returns<Expression<Func<PointsAward, bool>>, Expression<Func<PointsAward, object>>[]>((pred, _) =>
                awards.Where(pred.Compile()).ToList());

        var result = _logic.GetDailyTop().ToList();

        result.Should().HaveCount(1);
        result[0].TotalPoints.Should().Be(20);
    }

    [TestMethod]
    public void GetDailyTop_Respects_Limit_And_Defaults_When_Invalid()
    {
        var now = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        _clock.Setup(c => c.Now()).Returns(now);

        var strat = new ScoringStrategyMeta { Id = Guid.NewGuid(), Name = "Default", IsActive = true, IsDeleted = false, FilePath = "test/path", FileName = "test.dll" };
        _strategy.Setup(s => s.GetActiveOrThrow()).Returns(strat);

        var awards = Enumerable.Range(0, 50).Select(i =>
        {
            var u = new User($"U{i}", "X", $"u{i}@x", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard);
            return new PointsAward
            {
                Id = Guid.NewGuid(),
                UserId = u.Id,
                User = u,
                StrategyId = strat.Id,     // important: must match active strategy
                Points = 100 - (i * 5),
                At = now,
                Reason = "Test reason"
            };
        }).ToList();

        _pointsRepo
            .Setup(r => r.FindAll(
                It.IsAny<Expression<Func<PointsAward, bool>>>(),
                It.IsAny<Expression<Func<PointsAward, object>>[]>()))
            .Returns<Expression<Func<PointsAward, bool>>, Expression<Func<PointsAward, object>>[]>((pred, _) =>
                awards.Where(pred.Compile()).ToList());

        _logic.GetDailyTop(5).Should().HaveCount(5);
        _logic.GetDailyTop(-1).Should().HaveCount(10);

        _clock.VerifyAll();
        _pointsRepo.Verify(r => r.FindAll(
            It.IsAny<Expression<Func<PointsAward, bool>>>(),
            It.IsAny<Expression<Func<PointsAward, object>>[]>()), Times.Exactly(2));
    }
}

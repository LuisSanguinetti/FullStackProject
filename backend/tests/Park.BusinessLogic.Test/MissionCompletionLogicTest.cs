using System;
using Domain;
using FluentAssertions;
using IDataAccess;
using IParkBusinessLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Park.BusinessLogic;

namespace Park.BusinessLogic.Test;

[TestClass]
public class MissionCompletionLogicTest
{
    private Mock<IMissionLogic> _missions = null!;
    private Mock<IUserLogic> _users = null!;
    private Mock<IAwardPointsLogic> _awards = null!;
    private Mock<IRepository<MissionCompletion>> _repo = null!;
    private MissionCompletionLogic _logic = null!;

    [TestInitialize]
    public void Setup()
    {
        _missions = new Mock<IMissionLogic>(MockBehavior.Strict);
        _users = new Mock<IUserLogic>(MockBehavior.Strict);
        _awards = new Mock<IAwardPointsLogic>(MockBehavior.Strict);
        _repo = new Mock<IRepository<MissionCompletion>>(MockBehavior.Strict);

        _repo
            .Setup(r => r.Find(
                It.IsAny<System.Linq.Expressions.Expression<Func<MissionCompletion, bool>>>()))
            .Returns((MissionCompletion?)null);

        _logic = new MissionCompletionLogic(_missions.Object, _users.Object, _awards.Object, _repo.Object);
    }

    [TestMethod]
    public void Register_Computes_Points_Adds_To_User_Persists_Completion_And_Returns_Points()
    {
        // Arranges
        var userId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var strategyId = Guid.NewGuid();
        var when = new DateTime(2025, 10, 8, 12, 34, 56, DateTimeKind.Utc);

        var user = new User("Alice", "Baker", "alice@example.com", "p@ss", new DateOnly(1990, 1, 1), MembershipLevel.Standard)
        {
            Id = userId,
            Points = 0
        };
        var mission = new Mission("M1", "Desc", 10) { Id = missionId };

        const int computedPoints = 42;

        _users.Setup(u => u.GetOrThrow(userId)).Returns(user);
        _missions.Setup(m => m.GetOrThrow(missionId)).Returns(mission);

        _awards
            .Setup(a => a.ComputeForMission(user, mission, when, strategyId))
            .Returns(computedPoints);

        _users
            .Setup(u => u.AddPoints(userId, computedPoints));

        _repo
            .Setup(r => r.Add(It.IsAny<MissionCompletion>()))
            .Returns<MissionCompletion>(mc => mc);

        // Act
        var result = _logic.Register(userId, missionId, when, strategyId);

        // Assert
        result.Should().Be(computedPoints);

        _awards.Verify(a => a.ComputeForMission(user, mission, when, strategyId), Times.Once);
        _users.Verify(u => u.AddPoints(userId, computedPoints), Times.Once);

        _repo.Verify(r => r.Add(It.Is<MissionCompletion>(mc =>
            mc.UserId == userId &&
            mc.MissionId == missionId &&
            mc.DateCompleted == when &&
            mc.Points == computedPoints &&
            mc.Id != Guid.Empty)), Times.Once);

        _missions.VerifyAll();
        _users.VerifyAll();
        _awards.VerifyAll();
        _repo.VerifyAll();
    }
}

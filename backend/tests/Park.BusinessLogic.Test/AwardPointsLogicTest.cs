using Domain;
using IDataAccess;
using IParkBusinessLogic;
using Moq;

namespace Park.BusinessLogic.Test;

[TestClass]
public class AwardPointsLogicTest
{
    private Mock<IScoringStrategyMetaLogic> _meta = null!;
    private Mock<IRepository<PointsAward>> _pointsRepo = null!;
    private Mock<IAttractionHelperLogic> _attractionLogic = null!;
    private IAwardPointsLogic _logic = null!;

    [TestInitialize]
    public void Setup()
    {
        _meta = new Mock<IScoringStrategyMetaLogic>(MockBehavior.Strict);
        _pointsRepo = new Mock<IRepository<PointsAward>>(MockBehavior.Strict);
        _attractionLogic = new Mock<IAttractionHelperLogic>(MockBehavior.Strict);
        _logic = new AwardPointsLogic(_meta.Object, _pointsRepo.Object, _attractionLogic.Object);
    }

        private static User DummyUser() => new User
    {
        Id = Guid.NewGuid(),
        Name = "Jane",
        Surname = "Doe",
        Email = $"jane{Guid.NewGuid():N}@mail.com",
        Password = "hashed",
        DateOfBirth = new DateOnly(1990, 1, 1),
        Membership = MembershipLevel.Standard,
        Points = 0
    };

    private static AccessRecord DummyAccess()
    {
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var attractionId = Guid.NewGuid();

        return new AccessRecord
        {
            Id = Guid.NewGuid(),
            InAt = DateTime.UtcNow,

            TicketId = ticketId,
            Ticket = new Ticket
            {
                Id = ticketId,
                UserId = userId,
                Owner = new User
                {
                    Id = userId,
                    Name = "Jane",
                    Surname = "Doe",
                    Email = $"jane{Guid.NewGuid():N}@mail.com",
                    Password = "hashed",
                    DateOfBirth = new DateOnly(1990, 1, 1),
                    Membership = MembershipLevel.Standard,
                    Points = 0
                },
                QrCode = Guid.NewGuid(),
                VisitDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Type = TicketType.General
            },

            AttractionId = attractionId,
            Attraction = new Attraction
            {
                Id = attractionId,
                Name = "Coaster",
                Type = AttractionType.RollerCoaster,
                MinAge = 0,
                MaxCapacity = 10,
                Description = "Test"
            },
        };
    }

    private static Mission DummyMission() => new Mission
    {
        Id = Guid.NewGuid(),
        Title = "Ride 3 attractions",
        Description = "Do 3 rides",
        BasePoints = 10
    };

    [TestMethod]
    public void ComputeForAccess_Throws_WhenActiveStrategyHasNoFilePath()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        _meta.Setup(m => m.GetActiveOrThrowById(strategyId))
            .Returns(new ScoringStrategyMeta
            {
                Id = Guid.NewGuid(),
                Name = "NoPath",
                FilePath = string.Empty,
                FileName = "strategy.dll",
                IsActive = true,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow
            });

        // Act + Assert
        var ex = Assert.ThrowsException<InvalidOperationException>(
            () => _logic.ComputeForAccess(DummyUser(), DummyAccess(), DateTime.UtcNow, strategyId));
        StringAssert.Contains(ex.Message, "Active strategy has no file path.");

        _meta.Verify(m => m.GetActiveOrThrowById(strategyId), Times.Once);
    }

    [TestMethod]
    public void ComputeForMission_Throws_WhenPluginPathHasNoDirectory()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        _meta.Setup(m => m.GetActiveOrThrowById(strategyId))
            .Returns(new ScoringStrategyMeta
            {
                Id = Guid.NewGuid(),
                Name = "BadPath",
                FilePath = "strategy.dll",
                FileName = "strategy.dll",
                IsActive = true,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow
            });

     // Act + Assert
        var ex = Assert.ThrowsException<InvalidOperationException>(
            () => _logic.ComputeForMission(DummyUser(), DummyMission(), DateTime.UtcNow, strategyId));
        StringAssert.Contains(ex.Message, "Invalid plugin file path.");

        _meta.Verify(m => m.GetActiveOrThrowById(strategyId), Times.Once);
    }

    [TestMethod]
    public void ResolveActiveStrategy_Throws_When_No_Strategy_In_Folder()
    {
        // Arrange: create an empty folder to simulate "no implementations found"
        var emptyDir = Path.Combine(Path.GetTempPath(), "plugins_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(emptyDir);

        var strategyId = Guid.NewGuid();
        _meta.Setup(m => m.GetActiveOrThrowById(strategyId)).Returns(new ScoringStrategyMeta
        {
            Id = Guid.NewGuid(),
            Name = "EmptyFolder",
            FilePath = Path.Combine(emptyDir, "any.dll"),
            FileName = "any.dll",
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow
        });

        // Act + Assert
        var ex = Assert.ThrowsException<InvalidOperationException>(
            () => _logic.ComputeForAccess(DummyUser(), DummyAccess(), DateTime.UtcNow, strategyId));
        StringAssert.Contains(ex.Message, "does not contain an IScoringStrategy.");

        _meta.Verify(m => m.GetActiveOrThrowById(strategyId), Times.Once);
    }

    [TestMethod]
    public void CreatePointsAward_ForAccess_Uses_Attraction_Name_And_Persists()
    {
        // arrange
        var user = DummyUser();
        var access = DummyAccess();
        var when = DateTime.UtcNow;
        const int pts = 50;

        var strategyId = Guid.NewGuid();
        _meta.Setup(m => m.GetActiveOrThrowById(strategyId)).Returns(new ScoringStrategyMeta
        {
            Id = Guid.NewGuid(),
            Name = "Active",
            FilePath = @"C:\fake\plugin\strategy.dll", // not used in this test path
            FileName = "strategy.dll",
            IsActive = true,
            IsDeleted = false,
            CreatedOn = when
        });

        _attractionLogic
            .Setup(a => a.GetOrThrow(access.AttractionId))
            .Returns(access.Attraction!);

        _pointsRepo
            .Setup(r => r.Add(It.IsAny<PointsAward>()))
            .Returns<PointsAward>(p => p);

        var sut = (AwardPointsLogic)_logic;
        var mi = typeof(AwardPointsLogic).GetMethod("CreatePointsAward",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.IsNotNull(mi, "CreatePointsAward(reflection) not found");

        // act
        mi.Invoke(sut, new object[] { user, null, access, when, pts, strategyId });

        // assert
        _attractionLogic.Verify(a => a.GetOrThrow(access.AttractionId), Times.Once);
        _pointsRepo.Verify(r => r.Add(It.Is<PointsAward>(p =>
            p.UserId == user.Id &&
            p.User == user &&
            p.Points == pts &&
            p.Reason.Contains(access.Attraction!.Name) &&
            p.Reason.Contains(pts.ToString()) &&
            p.StrategyId == strategyId &&
            p.At == when
        )), Times.Once);
        _pointsRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void CreatePointsAward_ForMission_Uses_Mission_Title_And_Persists_Without_Attraction_Lookup()
    {
        // arrange
        var user = DummyUser();
        var mission = DummyMission();
        var when = DateTime.UtcNow;
        const int pts = 30;
        var strategyId = Guid.NewGuid();
        _meta.Setup(m => m.GetActiveOrThrowById(strategyId)).Returns(new ScoringStrategyMeta
        {
            Id = Guid.NewGuid(),
            Name = "Active",
            FilePath = @"C:\fake\plugin\strategy.dll",
            FileName = "strategy.dll",
            IsActive = true,
            IsDeleted = false,
            CreatedOn = when
        });

        _pointsRepo
            .Setup(r => r.Add(It.IsAny<PointsAward>()))
            .Returns<PointsAward>(p => p);

        var sut = (AwardPointsLogic)_logic;
        var mi = typeof(AwardPointsLogic).GetMethod("CreatePointsAward",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.IsNotNull(mi, "CreatePointsAward(reflection) not found");

        // act
        mi!.Invoke(sut, new object[] { user, mission, null, when, pts, strategyId });

        // assert
        _attractionLogic.Verify(a => a.GetOrThrow(It.IsAny<Guid>()), Times.Never);
        _pointsRepo.Verify(r => r.Add(It.Is<PointsAward>(p =>
            p.UserId == user.Id &&
            p.User == user &&
            p.Points == pts &&
            p.Reason.Contains(mission.Title) &&
            p.StrategyId != Guid.Empty &&
            p.At == when)), Times.Once);
        _pointsRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void ComputeForMission_Using_TestPlugin_Records_Award_And_Returns_ExpectedPoints()
    {
        // arrange: resolve plugin dll under repo: tests/Park.BusinessLogic.Test/TestPlugIn/*.dll
        var dllPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "TestPlugIn", "Plugins.dll"));
        var strategyId = Guid.NewGuid();

        _meta.Setup(m => m.GetActiveOrThrowById(strategyId)).Returns(new ScoringStrategyMeta
        {
            Id = Guid.NewGuid(),
            Name = "TestPlugin",
            FilePath = dllPath,
            FileName = Path.GetFileName(dllPath),
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow
        });

        _pointsRepo.Setup(r => r.Add(It.IsAny<PointsAward>())).Returns<PointsAward>(p => p);

        var user = DummyUser();            // Membership = Standard
        var mission = DummyMission();      // BasePoints = 10

        // act
        var points = _logic.ComputeForMission(user, mission, DateTime.UtcNow, strategyId);

        // assert: EventBonusStrategy => basePoints(10) * 1.0 = 10
        Assert.AreEqual(10, points);
        _pointsRepo.Verify(r => r.Add(It.Is<PointsAward>(p =>
            p.UserId == user.Id &&
            p.Points == points &&
            p.Reason.Contains("mission", StringComparison.OrdinalIgnoreCase) &&
            p.At != default && p.StrategyId != Guid.Empty)), Times.Once);
        _meta.Verify(m => m.GetActiveOrThrowById(strategyId), Times.AtLeastOnce);
    }

    [TestMethod]
    public void ComputeForAccess_Using_TestPlugin_Records_Award_And_Returns_ExpectedPoints()
    {
        // arrange
        var dllPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "TestPlugIn", "Plugins.dll"));
        var strategyId = Guid.NewGuid();

        _meta.Setup(m => m.GetActiveOrThrowById(strategyId)).Returns(new ScoringStrategyMeta
        {
            Id = Guid.NewGuid(),
            Name = "TestPlugin",
            FilePath = dllPath,
            FileName = Path.GetFileName(dllPath),
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow
        });

        var access = DummyAccess();
        _attractionLogic.Setup(a => a.GetOrThrow(access.AttractionId)).Returns(access.Attraction!);
        _pointsRepo.Setup(r => r.Add(It.IsAny<PointsAward>())).Returns<PointsAward>(p => p);

        // act
        var points = _logic.ComputeForAccess(access.Ticket!.Owner!, access, DateTime.UtcNow, strategyId);

        // assert
        Assert.AreEqual(10, points);
        _attractionLogic.Verify(a => a.GetOrThrow(access.AttractionId), Times.Once);
        _pointsRepo.Verify(r => r.Add(It.Is<PointsAward>(p =>
            p.UserId == access.Ticket!.Owner!.Id &&
            p.Points == 10 &&
            p.Reason.Contains(access.Attraction!.Name, StringComparison.OrdinalIgnoreCase) &&
            p.Reason.Contains("awarded for going to attraction", StringComparison.OrdinalIgnoreCase) &&
            p.StrategyId != Guid.Empty &&
            p.At != default
        )), Times.Once);
        _meta.Verify(m => m.GetActiveOrThrowById(strategyId), Times.Once);
    }

    [TestMethod]
    public void ComputeForAccess_With_SpecialEvent_Bonus_Using_TestPlugin_Returns_ExpectedPoints()
    {
        // arrange
        var dllPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "TestPlugIn", "Plugins.dll"));
        var strategyId = Guid.NewGuid();
        _meta.Setup(m => m.GetActiveOrThrowById(strategyId)).Returns(new ScoringStrategyMeta
        {
            Id = Guid.NewGuid(),
            Name = "TestPlugin",
            FilePath = dllPath,
            FileName = Path.GetFileName(dllPath),
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow
        });

        var access = DummyAccess();
        access.SpecialEventId = Guid.NewGuid(); // triggers +15 bonus in plugin → 10 + 15 = 25
        _attractionLogic.Setup(a => a.GetOrThrow(access.AttractionId)).Returns(access.Attraction!);
        _pointsRepo.Setup(r => r.Add(It.IsAny<PointsAward>())).Returns<PointsAward>(p => p);

        // act
        var points = _logic.ComputeForAccess(access.Ticket!.Owner!, access, DateTime.UtcNow,strategyId);

        // assert
        Assert.AreEqual(25, points);
        _attractionLogic.Verify(a => a.GetOrThrow(access.AttractionId), Times.Once);
        _pointsRepo.Verify(r => r.Add(It.Is<PointsAward>(p =>
            p.UserId == access.Ticket!.Owner!.Id &&
            p.Points == 25 &&
            p.Reason.Contains(access.Attraction!.Name, StringComparison.OrdinalIgnoreCase)
        )), Times.Once);
        _meta.Verify(m => m.GetActiveOrThrowById(strategyId), Times.Once);
    }
}

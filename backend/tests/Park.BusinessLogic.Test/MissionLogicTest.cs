using System;
using System.Linq.Expressions;
using Domain;
using FluentAssertions;
using IDataAccess;
using IParkBusinessLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Park.BusinessLogic.Test;

[TestClass]
public class MissionLogicTest
{
    private Mock<IRepository<Mission>> _repoMock = null!;
    private MissionLogic _logic = null!;

    [TestInitialize]
    public void Setup()
    {
        _repoMock = new Mock<IRepository<Mission>>(MockBehavior.Strict);

        // Default: Find returns null unless arranged otherwise
        _repoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Mission, bool>>>()))
            .Returns((Mission?)null);

        _logic = new MissionLogic(_repoMock.Object);
    }

    [TestMethod]
    public void CreateMission_Valid_Adds_And_Returns_Entity_With_Id_And_Trimmed_Fields()
    {
        // Arrange
        var title = "  Title  ";
        var description = "  Desc  ";
        var basePoints = 10;

        _repoMock
            .Setup(r => r.Add(It.IsAny<Mission>()))
            .Returns<Mission>(m => m);

        // Act
        var created = _logic.CreateMission(title, description, basePoints);

        // Assert
        created.Should().NotBeNull();
        created.Id.Should().NotBe(Guid.Empty);
        created.Title.Should().Be("Title");
        created.Description.Should().Be("Desc");
        created.BasePoints.Should().Be(basePoints);

        _repoMock.Verify(r => r.Add(It.Is<Mission>(m =>
            m.Title == "Title" &&
            m.Description == "Desc" &&
            m.BasePoints == basePoints &&
            m.Id != Guid.Empty
        )), Times.Once);

        _repoMock.VerifyNoOtherCalls();
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void CreateMission_Throws_When_Title_Is_NullOrWhiteSpace(string? badTitle)
    {
        // Arrange
        // Act
        Action act = () => _logic.CreateMission(badTitle!, "desc", 0);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Title is required.*");

        _repoMock.VerifyNoOtherCalls();
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void CreateMission_Throws_When_Description_Is_NullOrWhiteSpace(string? badDesc)
    {
        // Arrange
        // Act
        Action act = () => _logic.CreateMission("name", badDesc!, 0);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Description is required.*");

        _repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void CreateMission_Throws_When_BasePoints_Is_Negative()
    {
        // Arrange
        // Act
        Action act = () => _logic.CreateMission("name", "desc", -1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithMessage("*BasePoints must be >= 0.*");

        _repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetOrThrow_Returns_Mission_When_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        var mission = new Mission("Title", "Desc", 5) { Id = id };

        _repoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Mission, bool>>>()))
            .Returns((Expression<Func<Mission, bool>> pred) =>
                pred.Compile()(mission) ? mission : null);

        // Act
        var result = _logic.GetOrThrow(id);

        // Assert
        result.Should().BeSameAs(mission);
        _repoMock.Verify(r => r.Find(It.IsAny<Expression<Func<Mission, bool>>>()), Times.Once);
        _repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetOrThrow_Throws_When_NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Mission, bool>>>()))
            .Returns((Mission?)null);

        // Act
        Action act = () => _logic.GetOrThrow(id);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Mission not found");

        _repoMock.Verify(r => r.Find(It.IsAny<Expression<Func<Mission, bool>>>()), Times.Once);
        _repoMock.VerifyNoOtherCalls();
    }
}

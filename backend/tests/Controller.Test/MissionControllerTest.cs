using System;
using System.Linq;
using FluentAssertions;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using obligatorio.WebApi.Controllers;
using obligatorio.WebApi.DTO;

namespace Controller.Test;

[TestClass]
public class MissionControllerTest
{
    private Mock<IMissionLogic> _logicMock = null!;
    private MissionController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _logicMock = new Mock<IMissionLogic>(MockBehavior.Strict);
        _controller = new MissionController(_logicMock.Object);
    }

    [TestMethod]
    public void Create_Returns_Ok_With_MissionDto_And_Calls_Logic()
    {
        // Arrange
        var dto = new MissionCreateDto
        {
            Title = "New Mission",
            Description = "Desc",
            BasePoints = 15
        };

        var entity = new Domain.Mission(dto.Title, dto.Description, dto.BasePoints)
        {
            Id = Guid.NewGuid()
        };

        _logicMock
            .Setup(l => l.CreateMission(dto.Title, dto.Description, dto.BasePoints))
            .Returns(entity);

        // Act
        var result = _controller.Create(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.Value.Should().BeOfType<MissionDto>();

        var res = (MissionDto)ok.Value!;
        res.Id.Should().Be(entity.Id);
        res.Title.Should().Be(dto.Title);
        res.Description.Should().Be(dto.Description);
        res.BasePoints.Should().Be(dto.BasePoints);

        _logicMock.Verify(l => l.CreateMission(dto.Title, dto.Description, dto.BasePoints), Times.Once);
        _logicMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Create_Propagates_Logic_Exception()
    {
        // Arrange
        var dto = new MissionCreateDto
        {
            Title = String.Empty,
            Description = "Desc",
            BasePoints = 10
        };

        _logicMock
            .Setup(l => l.CreateMission(dto.Title, dto.Description, dto.BasePoints))
            .Throws(new ArgumentException("Title is required."));

        // Act
        Action act = () => _controller.Create(dto);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Title is required.*");

        _logicMock.Verify(l => l.CreateMission(dto.Title, dto.Description, dto.BasePoints), Times.Once);
        _logicMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Controller_Has_ApiController_And_Route_Attributes()
    {
        // Arrange
        var t = typeof(MissionController);

        // Act
        var apiAttr = t.GetCustomAttributes(typeof(ApiControllerAttribute), inherit: true);
        var routeAttr = t.GetCustomAttributes(typeof(RouteAttribute), inherit: true)
                         .Cast<RouteAttribute>()
                         .FirstOrDefault();

        // Assert
        apiAttr.Should().NotBeNull();
        apiAttr.Length.Should().Be(1);

        routeAttr.Should().NotBeNull();
        routeAttr!.Template.Should().Be("api/v1/admin/missions");
    }

    [TestMethod]
    public void Create_Has_HttpPost_And_Admin_Auth_Attributes()
    {
        // Arrange
        var method = typeof(MissionController).GetMethod(nameof(MissionController.Create));
        method.Should().NotBeNull("Create method must exist");

        // Act
        var postAttr = method!.GetCustomAttributes(typeof(HttpPostAttribute), inherit: true);
        var authAttr = method!.GetCustomAttributes(inherit: true)
                              .FirstOrDefault(a => a.GetType().Name == "AuthAttribute");

        // Assert
        postAttr.Should().NotBeNull();
        postAttr.Length.Should().Be(1);

        authAttr.Should().NotBeNull("AuthAttribute is required on Create");

        // RoleRequired == "admin"
        var roleProp = authAttr!.GetType().GetProperty("RoleRequired");
        roleProp.Should().NotBeNull();

        var roleVal = roleProp!.GetValue(authAttr) as string;
        roleVal.Should().Be("admin");
    }
}

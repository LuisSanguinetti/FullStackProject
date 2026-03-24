using Domain;
using FluentAssertions;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using obligatorio.WebApi.Controllers;
using obligatorio.WebApi.DTO;
using Park.BusinessLogic.Exceptions;

namespace Controller.Test;

[TestClass]
public class UserControllerTest
{
    private Mock<IUserLogic> _logicMock = null!;
    private Mock<IUserRoleLogic> _userRoleLogicMock = null!;
    private Mock<ISessionLogic> _sessionLogicMock = null!;
    private UserController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _logicMock = new Mock<IUserLogic>(MockBehavior.Strict);
        _userRoleLogicMock = new Mock<IUserRoleLogic>(MockBehavior.Strict);
        _sessionLogicMock = new Mock<ISessionLogic>(MockBehavior.Strict);
        _controller = new UserController(_logicMock.Object, _userRoleLogicMock.Object);
    }

    [TestMethod]
    public void GetUsers_ReturnsDtos_FromLogicUsers()
    {
        // Arrange
        var u1 = new User("Alice", "Baker", "alice@ex.com", "p1", new DateOnly(1990, 1, 1));
        var u2 = new User("Bob", "Carson", "bob@ex.com", "p2", new DateOnly(1991, 2, 2));

        _logicMock
            .Setup(l => l.GetUsersPage(2, 5))
            .Returns(new List<User> { u1, u2 });

        // Act
        var dtos = _controller.GetUsers(page: 2, pageSize: 5).ToList();

        // Assert
        dtos.Should().HaveCount(2);
        dtos[0].Id.Should().Be(u1.Id);
        dtos[0].Name.Should().Be("Alice");
        dtos[0].Surname.Should().Be("Baker");
        dtos[0].Email.Should().Be("alice@ex.com");

        dtos[1].Id.Should().Be(u2.Id);
        dtos[1].Name.Should().Be("Bob");
        dtos[1].Surname.Should().Be("Carson");
        dtos[1].Email.Should().Be("bob@ex.com");

        _logicMock.Verify(l => l.GetUsersPage(2, 5), Times.Once);
        _logicMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetUsers_UsesDefaultQueryParams_WhenNotProvided()
    {
        _logicMock
            .Setup(l => l.GetUsersPage(1, 10))
            .Returns(new List<User>());

        var dtos = _controller.GetUsers().ToList();

        dtos.Should().BeEmpty();
        _logicMock.Verify(l => l.GetUsersPage(1, 10), Times.Once);
        _logicMock.VerifyNoOtherCalls();
    }

    // verifica el pasaje sin correccion de los datos
    [TestMethod]
    public void GetUsers_PassesRawQueryParams_ToLogic()
    {
        _logicMock
            .Setup(l => l.GetUsersPage(-1, 0))
            .Returns(new List<User>());

        var dtos = _controller.GetUsers(page: -1, pageSize: 0).ToList();

        dtos.Should().BeEmpty();
        _logicMock.Verify(l => l.GetUsersPage(-1, 0), Times.Once);
        _logicMock.VerifyNoOtherCalls();
    }

    // verifica el pasaje correcto cuando los params son correctos
    [TestMethod]
    public void GetUsers_PassesExplicitParams_ToLogic()
    {
        _logicMock
            .Setup(l => l.GetUsersPage(3, 25))
            .Returns(new List<User>());

        var dtos = _controller.GetUsers(page: 3, pageSize: 25).ToList();

        dtos.Should().BeEmpty();
        _logicMock.Verify(l => l.GetUsersPage(3, 25), Times.Once);
        _logicMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Register_Returns200_AndCallsLogicWithPayload()
    {
        // Arrange
        var dtoIn = new UserRegisterDto
        {
            Name = "Alice",
            Surname = "Baker",
            Email = "alice@ex.com",
            Password = "secret!",
            DateOfBirth = new DateOnly(1990, 1, 1)
        };

        _logicMock
            .Setup(l => l.RegisterVisitor(It.Is<User>(u =>
                u.Name == "Alice" &&
                u.Surname == "Baker" &&
                u.Email == "alice@ex.com" &&
                u.Password == "secret!" &&
                u.DateOfBirth == new DateOnly(1990, 1, 1)
            )))
            .Verifiable();

        // Act
        var action = _controller.Register(dtoIn);

        // Assert
        var result = action as OkResult; // ActionResult (no genérico) → no usar .Result
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);

        _logicMock.Verify(l => l.RegisterVisitor(It.Is<User>(u =>
            u.Name == "Alice" &&
            u.Surname == "Baker" &&
            u.Email == "alice@ex.com" &&
            u.Password == "secret!" &&
            u.DateOfBirth == new DateOnly(1990, 1, 1))));

        _logicMock.VerifyNoOtherCalls();
    }

        [TestMethod]
    public void Update_Returns200_AndCallsLogic()
    {
        // Arrange
        var dto = new UserUpdateDto
        {
            Name = "Alice",
            Surname = "Baker",
            Email = "alice@ex.com",
            Password = "p!",
            DateOfBirth = new DateOnly(1990, 1, 1)
        };

        var me = Guid.NewGuid();
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        _controller.ControllerContext.HttpContext.Items["CurrentUserId"] = me;
        _logicMock
            .Setup(l => l.EditProfile(me, dto.Name, dto.Surname, dto.Email, dto.Password, dto.DateOfBirth))
            .Verifiable(); // void → no Returns

        // Act
        var action = _controller.Update(dto);

        // Assert
        var ok = action as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
        ok.Value.Should().BeEquivalentTo(new { message = "user updated" });

        _logicMock.Verify(l => l.EditProfile(me, dto.Name, dto.Surname, dto.Email, dto.Password, dto.DateOfBirth), Times.Once);
        _logicMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Update_Propagates_DuplicateEmailException()
    {
        // Arrange
        var me = Guid.NewGuid();
        var dto = new UserUpdateDto
        {
            Email = "dup@ex.com"
        };

        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        _controller.ControllerContext.HttpContext.Items["CurrentUserId"] = me;
        _logicMock
            .Setup(l => l.EditProfile(me, dto.Name, dto.Surname, dto.Email, dto.Password, dto.DateOfBirth))
            .Throws(new DuplicateEmailException(dto.Email!));

        // Act
        Action act = () => _controller.Update(dto);

        // Assert (controller should just throw; GlobalExceptionFilter maps to 409)
        act.Should().Throw<DuplicateEmailException>();
        _logicMock.Verify(l => l.EditProfile(me, dto.Name, dto.Surname, dto.Email, dto.Password, dto.DateOfBirth), Times.Once);
        _logicMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Update_Propagates_KeyNotFoundException()
    {
        // Arrange
        var dto = new UserUpdateDto { Email = "lucho@mail.com" };
        var me = Guid.NewGuid();
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        _controller.ControllerContext.HttpContext.Items["CurrentUserId"] = me; // actor = current user
        _logicMock
            .Setup(l => l.EditProfile(me , dto.Name, dto.Surname, dto.Email, dto.Password, dto.DateOfBirth))
            .Throws(new KeyNotFoundException("User not found"));

        // Act
        Action act = () => _controller.Update(dto);

        // Assert (controller should just throw; GlobalExceptionFilter maps to 404)
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*not found*");

        _logicMock.Verify(l => l.EditProfile(me, dto.Name, dto.Surname, dto.Email, dto.Password, dto.DateOfBirth), Times.Once);
        _logicMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Update_Propagates_InvalidCredentials_When_NoCurrentUserId()
    {
        var dto = new UserUpdateDto { Email = "x@ex.com" };

        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        _logicMock
            .Setup(l => l.EditProfile(Guid.Empty, dto.Name, dto.Surname, dto.Email, dto.Password, dto.DateOfBirth))
            .Throws(new InvalidCredentialsException());

        Action act = () => _controller.Update(dto);

        act.Should().Throw<InvalidCredentialsException>();
        _logicMock.Verify(l => l.EditProfile(Guid.Empty, dto.Name, dto.Surname, dto.Email, dto.Password, dto.DateOfBirth), Times.Once);
        _logicMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetUserId_Returns200_WithUser()
    {
        var id = Guid.NewGuid();
        var user = new User("Ana", "Perez", "ana@x.com", "p", new DateOnly(2000, 1, 1))
        {
            Id = id
        };

        _logicMock
            .Setup(l => l.GetByIdOrThrow(id))
            .Returns(user);

        var action = _controller.GetUserId(id);

        var result = action as OkObjectResult;
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        result.Value.Should().BeEquivalentTo(UserGetDto.DomToDto(user));

        _logicMock.Verify(l => l.GetByIdOrThrow(id), Times.Once);
        _logicMock.VerifyNoOtherCalls();
        _userRoleLogicMock.VerifyNoOtherCalls();
        _sessionLogicMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetMyRole_ReturnsGeneral_When_NoCurrentUserId()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        _userRoleLogicMock
            .Setup(l => l.GetRoleByUserId(Guid.Empty))
            .Returns("general");

        var action = _controller.GetMyRole();

        var result = action as OkObjectResult;
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        result.Value.Should().BeEquivalentTo(new { role = "general" });

        _userRoleLogicMock.Verify(l => l.GetRoleByUserId(Guid.Empty), Times.Once);
        _logicMock.VerifyNoOtherCalls();
        _userRoleLogicMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetMyRole_ReturnsRole_When_CurrentUserId_Exists()
    {
        var userId = Guid.NewGuid();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.ControllerContext.HttpContext.Items["CurrentUserId"] = userId;

        _userRoleLogicMock
            .Setup(l => l.GetRoleByUserId(userId))
            .Returns("admin");

        var action = _controller.GetMyRole();

        var result = action as OkObjectResult;
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        result.Value.Should().BeEquivalentTo(new { role = "admin" });

        _userRoleLogicMock.Verify(l => l.GetRoleByUserId(userId), Times.Once);
        _logicMock.VerifyNoOtherCalls();
        _userRoleLogicMock.VerifyNoOtherCalls();
    }
}

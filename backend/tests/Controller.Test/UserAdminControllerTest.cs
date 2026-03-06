using FluentAssertions;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using obligatorio.WebApi.Controllers;
using obligatorio.WebApi.DTO;

namespace Controller.Test;

[TestClass]
public class UserAdminControllerTest
{
    [TestMethod]
    public void CreateVisitor_Returns201()
    {
        var mock = new Mock<IUserAdminLogic>(MockBehavior.Strict);
        mock.Setup(l => l.CreateVisitor(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>()))
            .Returns(new Domain.User("N", "S", "v@x", "p", new DateOnly(2000, 1, 1)));

        var ctl = new UserAdminController(mock.Object);
        var dto = new VisitorCreateDto { Name = "N", Surname = "S", Email = "v@x", Password = "p", DateOfBirth = DateOnly.MinValue};

        var res = ctl.CreateVisitor(dto) as CreatedAtActionResult;
        Assert.IsNotNull(res);
        mock.VerifyAll();
    }

    [TestMethod]
    public void CreateAdmin_Propagates_InvalidOperationException_When_DuplicateEmail()
    {
        var mock = new Mock<IUserAdminLogic>(MockBehavior.Strict);
        mock.Setup(l => l.CreateAdmin(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>()))
            .Throws(new InvalidOperationException("Email already exists"));

        var ctl = new UserAdminController(mock.Object);
        var dto = new AdminCreateDto { Name = "N", Surname = "S", Email = "dup@x", Password = "p", DateOfBirth = DateOnly.MinValue };

        Assert.ThrowsException<InvalidOperationException>(() => ctl.CreateAdmin(dto));
        mock.VerifyAll();
    }

    [TestMethod]
    public void CreateAdmin_Returns201()
    {
        var mock = new Mock<IUserAdminLogic>(MockBehavior.Strict);
        mock.Setup(l => l.CreateAdmin(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>()))
            .Returns(new Domain.User("N", "S", "a@x", "p", new DateOnly(2000, 1, 1)));

        var ctl = new UserAdminController(mock.Object);
        var dto = new AdminCreateDto { Name = "N", Surname = "S", Email = "a@x", Password = "p", DateOfBirth = DateOnly.MinValue };

        var res = ctl.CreateAdmin(dto) as CreatedAtActionResult;
        Assert.IsNotNull(res);
        mock.VerifyAll();
    }

    [TestMethod]
    public void CreateOperator_Returns201()
    {
        var mock = new Mock<IUserAdminLogic>(MockBehavior.Strict);
        mock.Setup(l => l.CreateOperator(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>()))
            .Returns(new Domain.User("N", "S", "o@x", "p", new DateOnly(2000, 1, 1)));

        var ctl = new UserAdminController(mock.Object);
        var dto = new OperatorCreateDto { Name = "N", Surname = "S", Email = "o@x", Password = "p", DateOfBirth = DateOnly.MinValue };

        var res = ctl.CreateOperator(dto) as CreatedAtActionResult;
        Assert.IsNotNull(res);
        mock.VerifyAll();
    }

    [TestMethod]
    public void UserGetDtos_Properties_SetAndGet()
    {
        var empty = string.Empty;
        var dto = new UserGetDtos(Guid.Empty, empty, empty, empty, empty)
        {
            Id = Guid.NewGuid(),
            Name = "TestName",
            Surname = "TestSurname",
            Email = "test@example.com",
            Role = "TestRole",
        };

        // Assert
        Assert.IsNotNull(dto.Id);
        Assert.AreEqual("TestName", dto.Name);
        Assert.AreEqual("TestSurname", dto.Surname);
        Assert.AreEqual("test@example.com", dto.Email);
        Assert.AreEqual("TestRole", dto.Role);
    }

    [TestMethod]
    public void UserGetDtos_Constructor_SetsAllProperties()
    {
        var id = Guid.NewGuid();
        var dto = new UserGetDtos(id, "John", "Doe", "john@test.com", "Admin");

        Assert.AreEqual(id, dto.Id);
        Assert.AreEqual("John", dto.Name);
        Assert.AreEqual("Doe", dto.Surname);
        Assert.AreEqual("john@test.com", dto.Email);
        Assert.AreEqual("Admin", dto.Role);
    }

    [TestMethod]
    public void Delete_ReturnsNoContent_WhenUserDeleted()
    {
        var id = Guid.NewGuid();

        var mock = new Mock<IUserAdminLogic>(MockBehavior.Strict);
        mock.Setup(l => l.Delete(id));

        var ctl = new UserAdminController(mock.Object);

        var res = ctl.Delete(id);

        res.Should().BeOfType<NoContentResult>();

        mock.Verify(l => l.Delete(id), Times.Once);
        mock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Delete_PropagatesException_WhenUserDoesNotExist()
    {
        var id = Guid.NewGuid();

        var mock = new Mock<IUserAdminLogic>(MockBehavior.Strict);
        mock.Setup(l => l.Delete(id)).Throws(new ArgumentException("User not found"));

        var ctl = new UserAdminController(mock.Object);

        Action act = () => ctl.Delete(id);

        act.Should().Throw<ArgumentException>().WithMessage("*User not found*");

        mock.Verify(l => l.Delete(id), Times.Once);
        mock.VerifyNoOtherCalls();
    }
}

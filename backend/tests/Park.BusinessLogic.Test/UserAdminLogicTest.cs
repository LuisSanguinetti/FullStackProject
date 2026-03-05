using System;
using System.Linq.Expressions;
using Domain;
using FluentAssertions;
using IDataAccess;
using IParkBusinessLogic;
using Moq;
using Park.BusinessLogic.Exceptions;

namespace Park.BusinessLogic.Test;

[TestClass]
public class UserAdminLogicTest
{
    private Mock<IRepository<User>> _userRepo = null!;
    private Mock<IUserRoleLogic> _userRoleLogic = null!;
    private IUserAdminLogic _logic = null!;

    [TestInitialize]
    public void Setup()
    {
        _userRepo = new Mock<IRepository<User>>(MockBehavior.Strict);
        _userRoleLogic = new Mock<IUserRoleLogic>(MockBehavior.Strict);

        _userRepo.Setup(r => r.Add(It.IsAny<User>()))
                 .Returns<User>(u => u);

        _logic = new Park.BusinessLogic.UserAdminLogic(_userRepo.Object, _userRoleLogic.Object);
    }

    [TestMethod]
    public void CreateVisitor_Valid_AddsUser_And_AssignsVisitorRole()
    {
        _userRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>())).Returns((User?)null);
        _userRepo.Setup(r => r.Add(It.IsAny<User>())).Returns<User>(u => u);

        _userRoleLogic.Setup(l => l.AssignRoleByName(It.IsAny<Guid>(), "Visitor"));

        var u = _logic.CreateVisitor("Ana", "Ríos", "ana@x", "p",
            new DateOnly(2000, 1, 1), MembershipLevel.Standard);

        u.Should().NotBeNull();
        u.Email.Should().Be("ana@x");
        u.Membership.Should().Be(MembershipLevel.Standard);

        _userRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
        _userRoleLogic.Verify(l => l.AssignRoleByName(u.Id, "Visitor"), Times.Once);
        _userRepo.VerifyAll();
        _userRoleLogic.VerifyAll();
    }

    [TestMethod]
    public void CreateAdmin_Fails_When_EmailAlreadyExists()
    {
        _userRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(new User("Existing", "User", "admin@x", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard));

        Action act = () => _logic.CreateAdmin("Juan", "Pérez", "admin@x", "p", new DateOnly(1995, 2, 2));

        act.Should().Throw<DuplicateEmailException>().WithMessage("*admin@x*");

        _userRepo.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _userRepo.VerifyNoOtherCalls();
        _userRoleLogic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void CreateVisitor_Propagates_When_Role_Not_Found()
    {
        _userRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>())).Returns((User?)null);
        _userRepo.Setup(r => r.Add(It.IsAny<User>())).Returns<User>(u => u);

        _userRoleLogic.Setup(l => l.AssignRoleByName(It.IsAny<Guid>(), "Visitor"))
                      .Throws(new InvalidOperationException("Role 'Visitor' not found"));

        Action act = () => _logic.CreateVisitor("Ana", "Ríos", "ana@x", "p",
                                new DateOnly(2000, 1, 1), MembershipLevel.Standard);

        act.Should().Throw<InvalidOperationException>().WithMessage("*Role 'Visitor' not found*");
    }

    [TestMethod]
    public void CreateOperator_Valid_AssignsOperatorRole()
    {
        _userRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>())).Returns((User?)null);
        _userRepo.Setup(r => r.Add(It.IsAny<User>())).Returns<User>(u => u);

        _userRoleLogic.Setup(l => l.AssignRoleByName(It.IsAny<Guid>(), "Operator"));

        var u = _logic.CreateOperator("Op", "One", "op@x", "p", new DateOnly(1999, 1, 1));

        _userRoleLogic.Verify(l => l.AssignRoleByName(u.Id, "Operator"), Times.Once);
    }

    [TestMethod]
    public void CreateVisitor_Fails_When_Email_IsNullOrWhitespace()
    {
        Action act = () => _logic.CreateVisitor("Ana", "Ríos", "   ", "p",
            new DateOnly(2000, 1, 1), MembershipLevel.Standard);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("email")
            .WithMessage("*Invalid email*");

        _userRepo.VerifyNoOtherCalls();
        _userRoleLogic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void CreateVisitor_Fails_When_Email_MissingAtSign()
    {
        Action act = () => _logic.CreateVisitor("Ana", "Ríos", "invalid.email", "p",
            new DateOnly(2000, 1, 1), MembershipLevel.Standard);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("email")
            .WithMessage("*Invalid email*");

        _userRepo.VerifyNoOtherCalls();
        _userRoleLogic.VerifyNoOtherCalls();
    }
}

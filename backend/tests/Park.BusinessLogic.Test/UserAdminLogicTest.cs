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

        _userRoleLogic.Setup(l => l.AssignRoleByName(It.IsAny<Guid>(), "visitor"));

        var u = _logic.CreateVisitor("Ana", "Ríos", "ana@x", "p",
            new DateOnly(2000, 1, 1));

        u.Should().NotBeNull();
        u.Email.Should().Be("ana@x");

        _userRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
        _userRepo.VerifyAll();
        _userRoleLogic.VerifyAll();
    }

    [TestMethod]
    public void CreateAdmin_Fails_When_EmailAlreadyExists()
    {
        _userRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(new User("Existing", "User", "admin@x", "p", new DateOnly(1990, 1, 1)));

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

        _userRoleLogic.Setup(l => l.AssignRoleByName(It.IsAny<Guid>(), "visitor"))
                      .Throws(new InvalidOperationException("Role 'Visitor' not found"));

        Action act = () => _logic.CreateVisitor("Ana", "Ríos", "ana@x", "p",
                                new DateOnly(2000, 1, 1));

        act.Should().Throw<InvalidOperationException>().WithMessage("*Role 'Visitor' not found*");
    }

    [TestMethod]
    public void CreateOperator_Valid_AssignsOperatorRole()
    {
        _userRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>())).Returns((User?)null);
        _userRepo.Setup(r => r.Add(It.IsAny<User>())).Returns<User>(u => u);

        _userRoleLogic.Setup(l => l.AssignRoleByName(It.IsAny<Guid>(), "operator"));

        var u = _logic.CreateOperator("Op", "One", "op@x", "p", new DateOnly(1999, 1, 1));

        _userRoleLogic.Verify(l => l.AssignRoleByName(u.Id, "operator"), Times.Once);
    }

    [TestMethod]
    public void CreateVisitor_Fails_When_Email_IsNullOrWhitespace()
    {
        Action act = () => _logic.CreateVisitor("Ana", "Ríos", "   ", "p",
            new DateOnly(2000, 1, 1));

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
            new DateOnly(2000, 1, 1));

        act.Should().Throw<ArgumentException>()
            .WithParameterName("email")
            .WithMessage("*Invalid email*");

        _userRepo.VerifyNoOtherCalls();
        _userRoleLogic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void CreateAdmin_Valid_AddsUser_And_AssignsAdminRole()
    {
        _userRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>())).Returns((User?)null);
        _userRepo.Setup(r => r.Add(It.IsAny<User>())).Returns<User>(u => u);
        _userRoleLogic.Setup(l => l.AssignRoleByName(It.IsAny<Guid>(), "admin"));

        var u = _logic.CreateAdmin("Juan", "Perez", "admin@x", "p", new DateOnly(2000, 1, 1));

        u.Should().NotBeNull();
        u.Email.Should().Be("admin@x");

        _userRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
        _userRoleLogic.Verify(l => l.AssignRoleByName(u.Id, "admin"), Times.Once);
        _userRepo.VerifyAll();
        _userRoleLogic.VerifyAll();
    }

    [TestMethod]
    public void CreateAdmin_Fails_When_Email_IsNullOrWhitespace()
    {
        Action act = () => _logic.CreateAdmin("Juan", "Perez", "   ", "p", new DateOnly(2000, 1, 1));

        act.Should().Throw<ArgumentException>()
            .WithParameterName("email")
            .WithMessage("*Invalid email*");

        _userRepo.VerifyNoOtherCalls();
        _userRoleLogic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void CreateOperator_Fails_When_EmailAlreadyExists()
    {
        _userRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(new User("Existing", "User", "op@x", "p", new DateOnly(1990, 1, 1)));

        Action act = () => _logic.CreateOperator("Op", "One", "op@x", "p", new DateOnly(1999, 1, 1));

        act.Should().Throw<DuplicateEmailException>().WithMessage("*op@x*");

        _userRepo.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _userRepo.VerifyNoOtherCalls();
        _userRoleLogic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void CreateOperator_Fails_When_Email_IsNullOrWhitespace()
    {
        Action act = () => _logic.CreateOperator("Op", "One", "   ", "p", new DateOnly(1999, 1, 1));

        act.Should().Throw<ArgumentException>()
            .WithParameterName("email")
            .WithMessage("*Invalid email*");

        _userRepo.VerifyNoOtherCalls();
        _userRoleLogic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void CreateVisitor_Fails_When_EmailAlreadyExists()
    {
        _userRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(new User("Existing", "User", "visitor@x", "p", new DateOnly(1990, 1, 1)));

        Action act = () => _logic.CreateVisitor("Ana", "Rios", "visitor@x", "p", new DateOnly(2000, 1, 1));

        act.Should().Throw<DuplicateEmailException>().WithMessage("*visitor@x*");

        _userRepo.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _userRepo.VerifyNoOtherCalls();
        _userRoleLogic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void CreateAdmin_Normalizes_Email_Before_Adding_User()
    {
        _userRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>())).Returns((User?)null);
        _userRepo.Setup(r => r.Add(It.IsAny<User>())).Returns<User>(u => u);
        _userRoleLogic.Setup(l => l.AssignRoleByName(It.IsAny<Guid>(), "admin"));

        var u = _logic.CreateAdmin("Juan", "Perez", "  ADMIN@X  ", "p", new DateOnly(2000, 1, 1));

        u.Email.Should().Be("admin@x");
        _userRepo.Verify(r => r.Add(It.Is<User>(x => x.Email == "admin@x")), Times.Once);
        _userRoleLogic.Verify(l => l.AssignRoleByName(u.Id, "admin"), Times.Once);
        _userRepo.VerifyAll();
        _userRoleLogic.VerifyAll();
    }

    [TestMethod]
    public void Delete_Deletes_User_When_User_Exists()
    {
        var id = Guid.NewGuid();
        var user = new User("Juan", "Perez", "a@x", "p", new DateOnly(2000, 1, 1)) { Id = id };

        _userRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Expression<Func<User, object>>[]>()))
            .Returns((Expression<Func<User, bool>> pred, Expression<Func<User, object>>[] includes) => pred.Compile()(user) ? user : null);
        _userRepo.Setup(r => r.Delete(id));

        _logic.Delete(id);

        _userRepo.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _userRepo.Verify(r => r.Delete(id), Times.Once);
        _userRepo.VerifyNoOtherCalls();
        _userRoleLogic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Delete_Throws_When_User_Does_Not_Exist()
    {
        var id = Guid.NewGuid();

        _userRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((User?)null);

        Action act = () => _logic.Delete(id);

        act.Should().Throw<ArgumentException>().WithMessage("User not found");

        _userRepo.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _userRepo.Verify(r => r.Delete(It.IsAny<Guid>()), Times.Never);
        _userRepo.VerifyNoOtherCalls();
        _userRoleLogic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetByIdOrThrow_Returns_User_When_Found()
    {
        var id = Guid.NewGuid();
        var user = new User("Juan", "Perez", "a@x", "p", new DateOnly(2000, 1, 1)) { Id = id };

        _userRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Expression<Func<User, object>>[]>()))
            .Returns((Expression<Func<User, bool>> pred, Expression<Func<User, object>>[] includes) => pred.Compile()(user) ? user : null);

        var result = _logic.GetByIdOrThrow(id);

        result.Should().BeSameAs(user);
        _userRepo.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _userRepo.VerifyNoOtherCalls();
        _userRoleLogic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetByIdOrThrow_Throws_When_Not_Found()
    {
        var id = Guid.NewGuid();

        _userRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((User?)null);

        Action act = () => _logic.GetByIdOrThrow(id);

        act.Should().Throw<KeyNotFoundException>().WithMessage($"*{id}*");

        _userRepo.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _userRepo.VerifyNoOtherCalls();
        _userRoleLogic.VerifyNoOtherCalls();
    }
}

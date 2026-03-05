using System;
using System.Linq.Expressions;
using Domain;
using FluentAssertions;
using IDataAccess;
using IParkBusinessLogic;
using Moq;

namespace Park.BusinessLogic.Test;

[TestClass]
public class UserRoleLogicTest
{
    private Mock<IRepository<Role>> _roleRepo = null!;
    private Mock<IRepository<UserRole>> _userRoleRepo = null!;
    private IUserRoleLogic _logic = null!;

    [TestInitialize]
    public void Setup()
    {
        _roleRepo = new Mock<IRepository<Role>>(MockBehavior.Strict);
        _userRoleRepo = new Mock<IRepository<UserRole>>(MockBehavior.Strict);
        _logic = new Park.BusinessLogic.UserRoleLogic(_roleRepo.Object, _userRoleRepo.Object);
    }

    [TestMethod]
    public void AssignRoleByName_Creates_Link_When_Role_Exists_And_Not_Assigned()
    {
        var userId = Guid.NewGuid();
        var role = new Role { Id = Guid.NewGuid(), Name = "Admin" };

        _roleRepo.Setup(r => r.Find(It.IsAny<Expression<Func<Role, bool>>>()))
                 .Returns((Expression<Func<Role, bool>> pred) => pred.Compile()(role) ? role : null);

        _userRoleRepo.Setup(r => r.Find(It.IsAny<Expression<Func<UserRole, bool>>>()))
                     .Returns((UserRole?)null);

        _userRoleRepo.Setup(r => r.Add(It.IsAny<UserRole>()))
                     .Returns<UserRole>(ur => ur);

        _logic.AssignRoleByName(userId, "Admin");

        _userRoleRepo.Verify(r => r.Add(It.Is<UserRole>(x => x.UserId == userId && x.RoleId == role.Id)), Times.Once);
        _roleRepo.VerifyAll();
        _userRoleRepo.VerifyAll();
    }

    [TestMethod]
    public void AssignRoleByName_Throws_When_Role_Not_Found()
    {
        _roleRepo.Setup(r => r.Find(It.IsAny<Expression<Func<Role, bool>>>()))
                 .Returns((Role?)null);

        Action act = () => _logic.AssignRoleByName(Guid.NewGuid(), "NoExiste");
        act.Should().Throw<InvalidOperationException>().WithMessage("*Role*not found*");
    }

    [TestMethod]
    public void AssignRoleByName_Throws_When_Already_Assigned()
    {
        var userId = Guid.NewGuid();
        var role = new Role { Id = Guid.NewGuid(), Name = "Operator" };

        _roleRepo.Setup(r => r.Find(It.IsAny<Expression<Func<Role, bool>>>()))
                 .Returns(role);

        _userRoleRepo.Setup(r => r.Find(It.IsAny<Expression<Func<UserRole, bool>>>()))
                     .Returns(new UserRole { Id = Guid.NewGuid(), UserId = userId, RoleId = role.Id });

        Action act = () => _logic.AssignRoleByName(userId, "Operator");
        act.Should().Throw<InvalidOperationException>().WithMessage("*already has role*");
    }
}

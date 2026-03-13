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

        _roleRepo.Setup(r => r.Find(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<Expression<Func<Role, object>>[]>()))
                 .Returns((Expression<Func<Role, bool>> pred, Expression<Func<Role, object>>[] includes) => pred.Compile()(role) ? role : null);

        _userRoleRepo.Setup(r => r.Find(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<Expression<Func<UserRole, object>>[]>()))
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

    [TestMethod]
    public void AssignRoleByName_Throws_When_RoleName_Is_Null()
    {
        Action act = () => _logic.AssignRoleByName(Guid.NewGuid(), null!);

        act.Should().Throw<ArgumentException>().WithMessage("Role name required");

        _roleRepo.Verify(r => r.Find(It.IsAny<Expression<Func<Role, bool>>>()), Times.Never);
        _userRoleRepo.Verify(r => r.Find(It.IsAny<Expression<Func<UserRole, bool>>>()), Times.Never);
        _userRoleRepo.Verify(r => r.Add(It.IsAny<UserRole>()), Times.Never);
        _roleRepo.VerifyNoOtherCalls();
        _userRoleRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void AssignRoleByName_Throws_When_RoleName_Is_Whitespace()
    {
        Action act = () => _logic.AssignRoleByName(Guid.NewGuid(), "   ");

        act.Should().Throw<ArgumentException>().WithMessage("Role name required");

        _roleRepo.Verify(r => r.Find(It.IsAny<Expression<Func<Role, bool>>>()), Times.Never);
        _userRoleRepo.Verify(r => r.Find(It.IsAny<Expression<Func<UserRole, bool>>>()), Times.Never);
        _userRoleRepo.Verify(r => r.Add(It.IsAny<UserRole>()), Times.Never);
        _roleRepo.VerifyNoOtherCalls();
        _userRoleRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetRoleByUserId_ReturnsGeneral_When_UserId_Is_Empty()
    {
        var result = _logic.GetRoleByUserId(Guid.Empty);

        result.Should().Be("general");

        _roleRepo.Verify(r => r.Find(It.IsAny<Expression<Func<Role, bool>>>()), Times.Never);
        _userRoleRepo.Verify(r => r.Find(It.IsAny<Expression<Func<UserRole, bool>>>()), Times.Never);
        _roleRepo.VerifyNoOtherCalls();
        _userRoleRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetRoleByUserId_ReturnsAdmin_When_User_Has_Admin_Role()
    {
        var userId = Guid.NewGuid();
        var adminRole = new Role { Id = Guid.NewGuid(), Name = "admin" };

        _roleRepo
            .Setup(r => r.Find(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<Expression<Func<Role, object>>[]>()))
            .Returns((Expression<Func<Role, bool>> pred, Expression<Func<Role, object>>[] includes) => pred.Compile()(adminRole) ? adminRole : null);

        _userRoleRepo
            .Setup(r => r.Find(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<Expression<Func<UserRole, object>>[]>()))
            .Returns((Expression<Func<UserRole, bool>> pred, Expression<Func<UserRole, object>>[] includes) =>
            {
                var adminLink = new UserRole { Id = Guid.NewGuid(), UserId = userId, RoleId = adminRole.Id };
                return pred.Compile()(adminLink) ? adminLink : null;
            });

        var result = _logic.GetRoleByUserId(userId);

        result.Should().Be("admin");
        _roleRepo.Verify(r => r.Find(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<Expression<Func<Role, object>>[]>()), Times.Once);
        _userRoleRepo.Verify(r => r.Find(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<Expression<Func<UserRole, object>>[]>()), Times.Once);
        _roleRepo.VerifyNoOtherCalls();
        _userRoleRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetRoleByUserId_ReturnsOperator_When_Admin_Not_Found_And_User_Has_Operator_Role()
    {
        var userId = Guid.NewGuid();
        var operatorRole = new Role { Id = Guid.NewGuid(), Name = "operator" };

        _roleRepo
            .Setup(r => r.Find(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<Expression<Func<Role, object>>[]>()))
            .Returns((Expression<Func<Role, bool>> pred, Expression<Func<Role, object>>[] includes) => pred.Compile()(operatorRole) ? operatorRole : null);

        _userRoleRepo
            .Setup(r => r.Find(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<Expression<Func<UserRole, object>>[]>()))
            .Returns((Expression<Func<UserRole, bool>> pred, Expression<Func<UserRole, object>>[] includes) =>
            {
                var operatorLink = new UserRole { Id = Guid.NewGuid(), UserId = userId, RoleId = operatorRole.Id };
                return pred.Compile()(operatorLink) ? operatorLink : null;
            });

        var result = _logic.GetRoleByUserId(userId);

        result.Should().Be("operator");
        _roleRepo.Verify(r => r.Find(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<Expression<Func<Role, object>>[]>()), Times.Exactly(2));
        _userRoleRepo.Verify(r => r.Find(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<Expression<Func<UserRole, object>>[]>()), Times.Once);
        _roleRepo.VerifyNoOtherCalls();
        _userRoleRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetRoleByUserId_ReturnsGeneral_When_Admin_And_Operator_Do_Not_Exist()
    {
        var userId = Guid.NewGuid();

        _roleRepo
            .Setup(r => r.Find(It.IsAny<Expression<Func<Role, bool>>>()))
            .Returns((Role?)null);

        var result = _logic.GetRoleByUserId(userId);

        result.Should().Be("general");
        _roleRepo.Verify(r => r.Find(It.IsAny<Expression<Func<Role, bool>>>()), Times.Exactly(2));
        _userRoleRepo.Verify(r => r.Find(It.IsAny<Expression<Func<UserRole, bool>>>()), Times.Never);
        _roleRepo.VerifyNoOtherCalls();
        _userRoleRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetRoleByUserId_ReturnsGeneral_When_Admin_And_Operator_Exist_But_User_Has_None()
    {
        var userId = Guid.NewGuid();
        var adminRole = new Role { Id = Guid.NewGuid(), Name = "admin" };
        var operatorRole = new Role { Id = Guid.NewGuid(), Name = "operator" };

        _roleRepo
            .Setup(r => r.Find(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<Expression<Func<Role, object>>[]>()))
            .Returns((Expression<Func<Role, bool>> pred, Expression<Func<Role, object>>[] includes) =>
            {
                if(pred.Compile()(adminRole))
                {
                    return adminRole;
                }

                if(pred.Compile()(operatorRole))
                {
                    return operatorRole;
                }

                return null;
            });

        _userRoleRepo
            .Setup(r => r.Find(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<Expression<Func<UserRole, object>>[]>()))
            .Returns((UserRole?)null);

        var result = _logic.GetRoleByUserId(userId);

        result.Should().Be("general");
        _roleRepo.Verify(r => r.Find(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<Expression<Func<Role, object>>[]>()), Times.Exactly(2));
        _userRoleRepo.Verify(r => r.Find(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<Expression<Func<UserRole, object>>[]>()), Times.Exactly(2));
        _roleRepo.VerifyNoOtherCalls();
        _userRoleRepo.VerifyNoOtherCalls();
    }
}

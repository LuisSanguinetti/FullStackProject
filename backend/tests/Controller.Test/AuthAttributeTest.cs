using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using FluentAssertions;
using IDataAccess;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Park.BusinessLogic.Exceptions;

namespace Controller.Test;

[TestClass]
public class AuthAttributeTest
{
        private static AuthorizationFilterContext MakeCtx(
        string? authHeader,
        ISessionLogic sessionLogic,
        IRepository<UserRole> userRoleRepo)
    {
        var http = new DefaultHttpContext();
        if (!string.IsNullOrEmpty(authHeader))
        {
            http.Request.Headers["Authorization"] = authHeader;
        }

        var services = new ServiceCollection()
            .AddSingleton(sessionLogic)
            .AddSingleton(userRoleRepo)
            .BuildServiceProvider();

        http.RequestServices = services;

        var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(
            http, new RouteData(), new ActionDescriptor());

        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    [TestMethod]
    public async Task Auth_Allows_When_TokenValid_And_NoRoleRequired()
    {
        var user = new User("Alice", "Baker", "a@x", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard) { Id = Guid.NewGuid() };
        var token = Guid.NewGuid();

        var sessionMock = new Mock<ISessionLogic>(MockBehavior.Strict);
        sessionMock.Setup(s => s.GetUserBySession(token)).Returns(user);

        var rolesMock = new Mock<IRepository<UserRole>>(MockBehavior.Strict);

        var attr = new AuthAttribute(); // no role required
        var ctx = MakeCtx($"Bearer {token}", sessionMock.Object, rolesMock.Object);

        Func<Task> act = async () => await attr.OnAuthorizationAsync(ctx);
        await act.Should().NotThrowAsync();

        ctx.HttpContext.Items["CurrentUserId"].Should().Be(user.Id);
        sessionMock.Verify(s => s.GetUserBySession(token), Times.Once);
        rolesMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task Auth_Throws401_When_MissingHeader()
    {
        var sessionMock = new Mock<ISessionLogic>(MockBehavior.Strict);
        var rolesMock = new Mock<IRepository<UserRole>>(MockBehavior.Strict);

        var attr = new AuthAttribute();
        var ctx = MakeCtx(null, sessionMock.Object, rolesMock.Object);

        Func<Task> act = async () => await attr.OnAuthorizationAsync(ctx);
        await act.Should().ThrowAsync<InvalidCredentialsException>();

        sessionMock.VerifyNoOtherCalls();
        rolesMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task Auth_Throws401_When_BadTokenFormat()
    {
        var sessionMock = new Mock<ISessionLogic>(MockBehavior.Strict);
        var rolesMock = new Mock<IRepository<UserRole>>(MockBehavior.Strict);

        var attr = new AuthAttribute();
        var ctx = MakeCtx("Bearer not-a-guid", sessionMock.Object, rolesMock.Object);

        Func<Task> act = async () => await attr.OnAuthorizationAsync(ctx);
        await act.Should().ThrowAsync<InvalidCredentialsException>();

        sessionMock.VerifyNoOtherCalls();
        rolesMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task Auth_Throws401_When_SessionNotFound()
    {
        var token = Guid.NewGuid();
        var sessionMock = new Mock<ISessionLogic>(MockBehavior.Strict);
        sessionMock.Setup(s => s.GetUserBySession(token)).Returns((User?)null);

        var rolesMock = new Mock<IRepository<UserRole>>(MockBehavior.Strict);

        var attr = new AuthAttribute();
        var ctx = MakeCtx(token.ToString(), sessionMock.Object, rolesMock.Object);

        Func<Task> act = async () => await attr.OnAuthorizationAsync(ctx);
        await act.Should().ThrowAsync<InvalidCredentialsException>();

        sessionMock.Verify(s => s.GetUserBySession(token), Times.Once);
        rolesMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task Auth_Allows_When_RoleRequired_ByName_Present()
    {
        var user = new User("Alice","Baker","a@x","p", new DateOnly(1990,1,1), MembershipLevel.Standard){ Id = Guid.NewGuid() };
        var token = Guid.NewGuid();

        var sessionMock = new Mock<ISessionLogic>(MockBehavior.Strict);
        sessionMock.Setup(s => s.GetUserBySession(token)).Returns(user);

        var rolesMock = new Mock<IRepository<UserRole>>(MockBehavior.Strict);
        rolesMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<UserRole, bool>>>()))
                 .Returns(new UserRole { UserId = user.Id, Role = new Role("admin") });

        var attr = new AuthAttribute { RoleRequired = "admin" };
        var ctx = MakeCtx($"Bearer {token}", sessionMock.Object, rolesMock.Object);

        Func<Task> act = async () => await attr.OnAuthorizationAsync(ctx);
        await act.Should().NotThrowAsync();

        sessionMock.Verify(s => s.GetUserBySession(token), Times.Once);
        rolesMock.Verify(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<UserRole, bool>>>()), Times.Once);
    }

    [TestMethod]
    public async Task Auth_Throws401_When_RoleRequired_ByName_Missing()
    {
        var user = new User("Alice","Baker","a@x","p", new DateOnly(1990,1,1), MembershipLevel.Standard){ Id = Guid.NewGuid() };
        var token = Guid.NewGuid();

        var sessionMock = new Mock<ISessionLogic>(MockBehavior.Strict);
        sessionMock.Setup(s => s.GetUserBySession(token)).Returns(user);

        var rolesMock = new Mock<IRepository<UserRole>>(MockBehavior.Strict);
        rolesMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<UserRole, bool>>>()))
                 .Returns((UserRole?)null);

        var attr = new AuthAttribute { RoleRequired = "admin" };
        var ctx = MakeCtx(token.ToString(), sessionMock.Object, rolesMock.Object);

        Func<Task> act = async () => await attr.OnAuthorizationAsync(ctx);
        await act.Should().ThrowAsync<InvalidCredentialsException>();

        sessionMock.Verify(s => s.GetUserBySession(token), Times.Once);
        rolesMock.Verify(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<UserRole, bool>>>()), Times.Once);
    }
}

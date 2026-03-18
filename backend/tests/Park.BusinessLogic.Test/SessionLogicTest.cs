using System.Linq.Expressions;
using Domain;
using FluentAssertions;
using IDataAccess;
using IParkBusinessLogic;
using Moq;

namespace Park.BusinessLogic.Test;

[TestClass]
public class SessionLogicTest
{
    private Mock<IRepository<Session>> _repoSessionMock = null!;
    private Mock<IUserLogic> _userLogicMock = null!;
    private SessionLogic _logic = null!;

    [TestInitialize]
    public void Setup()
    {
        _repoSessionMock = new Mock<IRepository<Session>>(MockBehavior.Strict);
        _userLogicMock = new Mock<IUserLogic>(MockBehavior.Strict);
        _logic = new SessionLogic(_repoSessionMock.Object, _userLogicMock.Object);
    }

    [TestMethod]
    public void CreateSession_CreatesAndPersistsSession_ReturnsSessionId()
    {
        // arrange
        var user = new User("Alice", "Baker", "alice@x", "p", new DateOnly(1990, 1, 1));

        Session? captured = null;
        _repoSessionMock
            .Setup(r => r.Add(It.IsAny<Session>()))
            .Callback((Session s) => captured = s)
            .Returns((Session s) => s)
            .Verifiable();

        // act
        var returnedId = _logic.CreateSession(user);

        // assert
        captured.Should().NotBeNull();
        captured!.UserId.Should().Be(user.Id);
        captured.User.Should().Be(user);
        captured.Token.Should().NotBe(Guid.Empty);

        returnedId.Should().Be(captured.Token);
        returnedId.Should().NotBe(Guid.Empty);

        _repoSessionMock.Verify(r => r.Add(It.Is<Session>(s =>
            s.UserId == user.Id && ReferenceEquals(s.User, user) && s.Token != Guid.Empty)), Times.Once);
        _repoSessionMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void DeleteSession_DeletesSession_WhenTokenExists()
    {
        // arrange
        var user = new User("Bob", "Carson", "bob@x", "p", new DateOnly(1991, 2, 2));
        var existing = new Session(user);

        _repoSessionMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Session, bool>>>()))
            .Returns(existing)
            .Verifiable();

        _repoSessionMock
            .Setup(r => r.Delete(existing.Id))
            .Verifiable();

        // act
        _logic.DeleteSession(existing.Token);

        // assert
        _repoSessionMock.Verify(r => r.Find(It.IsAny<Expression<Func<Session, bool>>>()), Times.Once);
        _repoSessionMock.Verify(r => r.Delete(existing.Id), Times.Once);
        _repoSessionMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void DeleteSession_Throws_WhenTokenNotFound()
    {
        var token = Guid.NewGuid();

        _repoSessionMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Session, bool>>>()))
            .Returns((Session?)null)
            .Verifiable();

        Action act = () => _logic.DeleteSession(token);

        act.Should().Throw<ArgumentException>().WithMessage("invalid token");

        _repoSessionMock.Verify(r => r.Find(It.IsAny<Expression<Func<Session, bool>>>()), Times.Once);
        _repoSessionMock.Verify(r => r.Delete(It.IsAny<Guid>()), Times.Never);
        _repoSessionMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetUserBySession_ReturnsUser_WhenSessionExists()
    {
        var token = Guid.NewGuid();
        var user = new User("Alice","Baker","a@x","p", new DateOnly(1990,1,1)) { Id = Guid.NewGuid() };
        var session = new Session(user) { Token = token, User = user, UserId = user.Id };

        _repoSessionMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Session, bool>>>(), It.IsAny<Expression<Func<Session, object>>[]>()))
            .Returns((Expression<Func<Session, bool>> pred, Expression<Func<Session, object>>[] includes) => pred.Compile()(session) ? session : null);
        _userLogicMock
            .Setup(l => l.GetByIdOrThrow(user.Id))
            .Returns(user);

        var result = _logic.GetUserBySession(token);

        result.Should().BeSameAs(user);
        _repoSessionMock.Verify(r => r.Find(It.IsAny<Expression<Func<Session, bool>>>(), It.IsAny<Expression<Func<Session, object>>[]>()), Times.Once);
    }

    [TestMethod]
    public void GetUserBySession_ReturnsNull_WhenSessionNotFound()
    {
        var token = Guid.NewGuid();

        _repoSessionMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Session, bool>>>()))
            .Returns((Session?)null);

        var result = _logic.GetUserBySession(token);

        result.Should().BeNull();
        _repoSessionMock.Verify(r => r.Find(It.IsAny<Expression<Func<Session, bool>>>()), Times.Once);
    }

    [TestMethod]
    public void DeleteSession_WithEmptyToken_ThrowsInvalidTokenException()
    {
        // arrange
        _repoSessionMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Session, bool>>>()))
            .Returns((Session?)null);

        // act
        Action act = () => _logic.DeleteSession(Guid.Empty);

        // assert
        act.Should().Throw<ArgumentException>().WithMessage("invalid token");
        _repoSessionMock.Verify(r => r.Find(It.IsAny<Expression<Func<Session, bool>>>()), Times.Once);
        _repoSessionMock.Verify(r => r.Delete(It.IsAny<Guid>()), Times.Never);
        _repoSessionMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void CreateSession_ReturnsToken_From_Added_Session()
    {
        var user = new User("Ana", "Perez", "ana@x", "p", new DateOnly(2000, 1, 1));
        var persisted = new Session(user) { Token = Guid.NewGuid() };

        _repoSessionMock
            .Setup(r => r.Add(It.IsAny<Session>()))
            .Returns(persisted);

        var result = _logic.CreateSession(user);

        result.Should().Be(persisted.Token);
        _repoSessionMock.Verify(r => r.Add(It.IsAny<Session>()), Times.Once);
        _repoSessionMock.VerifyNoOtherCalls();
    }
}

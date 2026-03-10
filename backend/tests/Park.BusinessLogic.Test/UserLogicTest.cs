using System.Linq.Expressions;
using Domain;
using FluentAssertions;
using IDataAccess;
using Moq;
using Park.BusinessLogic.Exceptions;

namespace Park.BusinessLogic.Test;

[TestClass]
public class UserLogicTest
{
    private Mock<IRepository<User>> _repoMock = null!;
    private Mock<IRepository<Session>> _repoSessionMock = null!;
    private UserLogic _logic = null!;
    private SessionLogic _sessionLogic = null!;

    [TestInitialize]
    public void Setup()
    {
        _repoMock = new Mock<IRepository<User>>(MockBehavior.Strict);
        _repoSessionMock = new Mock<IRepository<Session>>(MockBehavior.Strict);

        _repoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((User?)null);
        _repoSessionMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Session, bool>>>()))
            .Returns((Session?)null);

        _sessionLogic = new SessionLogic(_repoSessionMock.Object);
        _logic = new UserLogic(_repoMock.Object, _sessionLogic);
    }

    [TestMethod]
    public void GetUsersPage()
    {
        // arrange
        var expected = new List<User>
        {
            new User("Alice", "Baker", "alice@x", "p", new DateOnly(1990, 1, 1)),
            new User("Bob", "Carson", "bob@x", "p", new DateOnly(1991, 2, 2)),
        };

        _repoMock
            .Setup(r => r.GetPage(
                It.Is<int>(p => p == 2),
                It.Is<int>(s => s == 5),
                It.Is<Expression<Func<User, bool>>>(f => f == null),
                It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
            .Returns(expected);

        // act
        var result = _logic.GetUsersPage(page: 2, pageSize: 5);

        // assert
        result.Should().BeEquivalentTo(expected, opts => opts.WithStrictOrdering());
        _repoMock.Verify(r => r.GetPage(
            It.Is<int>(p => p == 2),
            It.Is<int>(s => s == 5),
            It.Is<Expression<Func<User, bool>>>(f => f == null),
            It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
            It.IsAny<Expression<Func<User, object>>[]>()), Times.Once);
        _repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetUsersPage_NormalizesPageAndDefaultSize_WhenInvalid()
    {
        _repoMock
            .Setup(r => r.GetPage(
                It.Is<int>(p => p == 1),
                It.Is<int>(s => s == 10),
                It.Is<Expression<Func<User, bool>>>(f => f == null),
                It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
            .Returns(new List<User>());

        // act
        var result = _logic.GetUsersPage(page: 0, pageSize: 0);

        // assert
        result.Should().BeEmpty();
        _repoMock.Verify(r => r.GetPage(
            It.Is<int>(p => p == 1),
            It.Is<int>(s => s == 10),
            It.Is<Expression<Func<User, bool>>>(f => f == null),
            It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
            It.IsAny<Expression<Func<User, object>>[]>()), Times.Once);
        _repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetUsersPage_MaxPage()
    {
        _repoMock
            .Setup(r => r.GetPage(
                It.Is<int>(p => p == 1),
                It.Is<int>(s => s == 15),
                It.Is<Expression<Func<User, bool>>>(f => f == null),
                It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
            .Returns(new List<User>());

        // act
        var result = _logic.GetUsersPage(page: 1, pageSize: 999);

        // assert
        result.Should().BeEmpty();
        _repoMock.Verify(r => r.GetPage(
            It.Is<int>(p => p == 1),
            It.Is<int>(s => s == 15),
            It.Is<Expression<Func<User, bool>>>(f => f == null),
            It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
            It.IsAny<Expression<Func<User, object>>[]>()), Times.Once);
        _repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetUsersPage_Oreder()
    {
        // arrange
        Func<IQueryable<User>, IOrderedQueryable<User>>? capturedOrderBy = null;

        _repoMock
            .Setup(r => r.GetPage(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                It.IsAny<Expression<Func<User, object>>[]>()))
            .Callback((int page, int size,
                       Expression<Func<User, bool>>? filter,
                       Func<IQueryable<User>, IOrderedQueryable<User>> orderBy,
                       Expression<Func<User, object>>[] includes) =>
            {
                capturedOrderBy = orderBy;
            })
            .Returns(new List<User>());

        // act
        _ = _logic.GetUsersPage(page: 1, pageSize: 5);

        // assert
        capturedOrderBy.Should().NotBeNull();

        var u1 = new User("Bob", "Carson", "1@x", "p", new DateOnly(1990, 1, 1)) { Id = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAA1") };
        var u2 = new User("Alice", "Carson", "2@x", "p", new DateOnly(1991, 2, 2)) { Id = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAA2") };
        var u3 = new User("Alice", "Baker", "3@x", "p", new DateOnly(1992, 3, 3)) { Id = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAA3") };
        var u4 = new User("Alice", "Baker", "4@x", "p", new DateOnly(1993, 4, 4)) { Id = Guid.Parse("00000000-0000-0000-0000-000000000004") };

        var input = new List<User> { u1, u2, u3, u4 }.AsQueryable();
        var ordered = capturedOrderBy!(input).ToList();

        ordered.Should().ContainInOrder(u4, u3, u2, u1);
    }

    [TestMethod]
    public void RegisterVisitor_AddsUser_ToRepository()
    {
        // arrange
        var user = new User("Alice", "Baker", "alice@x", "p", new DateOnly(1990, 1, 1));

        _repoMock
            .Setup(r => r.Add(It.Is<User>(u =>
                u.Name == "Alice" &&
                u.Surname == "Baker" &&
                u.Email == "alice@x" &&
                u.Password == "p" &&
                u.DateOfBirth == new DateOnly(1990, 1, 1))))
            .Returns((User u) => u)
            .Verifiable();

        // act
        _logic.RegisterVisitor(user);

        // assert
        _repoMock.VerifyAll();
    }

    [TestMethod]
    public void RegisterVisitor_PassesSameInstance()
    {
        // arrange
        var user = new User("Bob", "Carson", "bob@x", "p", new DateOnly(1991, 2, 2));

        _repoMock
            .Setup(r => r.Add(It.Is<User>(u => ReferenceEquals(u, user))))
            .Returns(user)
            .Verifiable();

        // act
        _logic.RegisterVisitor(user);

        // assert
        _repoMock.VerifyAll();
    }

    [TestMethod]
    public void RegisterVisitor_ThrowsDuplicateEmail_WhenEmailExists()
    {
        // arrange:
        var existing = new User("Eve", "Doe", "dup@x", "p", new DateOnly(1990,1,1));
        _repoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(existing);

        var newUser = new User("New", "User", "dup@x", "p", new DateOnly(1995,5,5));

        // act
        Action act = () => _logic.RegisterVisitor(newUser);

        // assert
        act.Should().Throw<Park.BusinessLogic.Exceptions.DuplicateEmailException>();
    }

    [TestMethod]
    public void Login_ReturnsToken_AndCreatesSession_WhenCredentialsValid()
    {
        // arrange
        var user = new User("Alice", "Baker", "alice@x", "p", new DateOnly(1990, 1, 1));

        // User repo:
        _repoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);

        // Session repo:
        _repoSessionMock
            .Setup(r => r.Add(It.IsAny<Session>()))
            .Returns((Session s) => s);

        // act
        var token = _logic.Login("alice@x", "p");

        // assert
        token.Should().NotBeNull();
        token!.Value.Should().NotBe(Guid.Empty);

        _repoMock.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.AtLeastOnce);
        _repoSessionMock.Verify(r => r.Add(It.Is<Session>(s => s.UserId == user.Id)), Times.Once);
        _repoSessionMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Login_Throws_WhenCredentialsInvalid()
    {
        // arrange: user not found
        _repoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((User?)null);

        // act
        Action act = () => _logic.Login("unknown@x", "bad");

        // assert
        act.Should().Throw<InvalidCredentialsException>()
            .WithMessage("Email or password is incorrect.");

        _repoMock.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.AtLeastOnce);
        _repoSessionMock.Verify(r => r.Add(It.IsAny<Session>()), Times.Never);
        _repoSessionMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Update_Throws_WhenUserDoesNotExist()
    {
        // arrange
        var me = Guid.NewGuid();
        var repo = new Mock<IRepository<User>>(MockBehavior.Strict);
        repo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((User?)null);

        var logic = new UserLogic(repo.Object, _sessionLogic);
        var id = Guid.NewGuid();

        // act
        Action act = () => logic.EditProfile(me,
            name: "X", surname: "Y", email: "x@y", password: "p", dateOfBirth: new DateOnly(2000, 1, 1));

        // assert
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*not found*");
        repo.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.AtLeastOnce);
        repo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void EditProfile_ThrowsDuplicateEmail_WhenEmailAlreadyUsed()
    {
        // arrange
        var id = Guid.NewGuid();
        var user = new User("Old", "User", "old@x", "oldp", new DateOnly(1990,1,1)) { Id = id };
        var dupe = new User("Someone", "Else", "new@x", "p", new DateOnly(1991,1,1)) { Id = Guid.NewGuid() };

        var repo = new Mock<IRepository<User>>(MockBehavior.Strict);
        repo.Setup(r => r.Find(It.IsAny<Expression<Func<User,bool>>>()))
            .Returns((Expression<Func<User,bool>> pred) =>
            {
                var f = pred.Compile();
                if (f(user))
                {
                    return user;
                }

                if (f(dupe))
                {
                    return dupe;
                }

                return null;
            });

        var logic = new UserLogic(repo.Object, _sessionLogic);
        var me = id;

        // act
        Action act = () => logic.EditProfile(
            me,
            name: null,
            surname: null,
            email: "new@x",
            password: null,
            dateOfBirth: null);

        // assert
        act.Should().Throw<DuplicateEmailException>();
        user.Email.Should().Be("old@x");
        repo.Verify(r => r.Find(It.IsAny<Expression<Func<User,bool>>>()), Times.AtLeastOnce);
        repo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void EditProfile_Updates_AllProvidedFields_WhenValid()
    {
        // arrange
        var id = Guid.NewGuid();
        var user = new User("Old", "User", "old@x", "oldp", new DateOnly(1990, 1, 1))
        {
            Id = id
        };

        var repo = new Mock<IRepository<User>>(MockBehavior.Strict);

        repo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((Expression<Func<User, bool>> pred) =>
            {
                var f = pred.Compile();
                return f(user) ? user : null;
            });

        repo.Setup(r => r.Update(It.Is<User>(u => u.Id == user.Id)))
            .Returns((User u) => u);

        var logic = new UserLogic(repo.Object, _sessionLogic);
        var me = id;

        // act
        logic.EditProfile(
            me,
            name: "NewName",
            surname: "NewSurname",
            email: "new@x",
            password: "newp",
            dateOfBirth: new DateOnly(2000, 2, 2));

        // assert
        user.Name.Should().Be("NewName");
        user.Surname.Should().Be("NewSurname");
        user.Email.Should().Be("new@x");
        user.Password.Should().Be("newp");
        user.DateOfBirth.Should().Be(new DateOnly(2000, 2, 2));

        repo.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Exactly(2));
        repo.Verify(r => r.Update(It.Is<User>(u => u.Id == user.Id)), Times.Once);
    }

    [TestMethod]
    public void EditProfile_ThrowsInvalidCredentials_WhenUserIdIsEmpty()
    {
        var repo = new Mock<IRepository<User>>(MockBehavior.Strict);
        var logic = new UserLogic(repo.Object, _sessionLogic);

        Action act = () => logic.EditProfile(Guid.Empty, null, null, null, null, null);

        act.Should().Throw<InvalidCredentialsException>();
        repo.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Never);
    }

    [TestMethod]
    public void GetByIdOrThrow_Returns_User_When_Found()
    {
        var id = Guid.NewGuid();
        var user = new User("Alice", "Baker", "a@x", "p", new DateOnly(1990,1,1)) { Id = id };

        _repoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((Expression<Func<User, bool>> pred) => pred.Compile()(user) ? user : null);

        var result = _logic.GetByIdOrThrow(id);

        result.Should().BeSameAs(user);
        _repoMock.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetByIdOrThrow_Throws_When_NotFound()
    {
        var id = Guid.NewGuid();

        _repoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((User?)null);

        Action act = () => _logic.GetByIdOrThrow(id);

        act.Should().Throw<KeyNotFoundException>().WithMessage($"*{id}*");
        _repoMock.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void IsEmailUnique_Returns_True_When_No_Match()
    {
        _repoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((User?)null);

        var ok = _logic.IsEmailUnique("noone@x");

        ok.Should().BeTrue();
        _repoMock.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void IsEmailUnique_Returns_False_When_Email_Exists()
    {
        var existing = new User("Eve", "Doe", "dup@x", "p", new DateOnly(1990,1,1));

        _repoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(existing);

        var ok = _logic.IsEmailUnique("dup@x");

        ok.Should().BeFalse();
        _repoMock.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Login_Throws_When_Blank_Email_Or_Password()
    {
        Action a1 = () => _logic.Login(String.Empty, "p");
        Action a2 = () => _logic.Login("   ", "p");
        Action a3 = () => _logic.Login("u@x", String.Empty);
        Action a4 = () => _logic.Login("u@x", "   ");

        a1.Should().Throw<InvalidCredentialsException>().WithMessage("Email or password is incorrect.");
        a2.Should().Throw<InvalidCredentialsException>().WithMessage("Email or password is incorrect.");
        a3.Should().Throw<InvalidCredentialsException>().WithMessage("Email or password is incorrect.");
        a4.Should().Throw<InvalidCredentialsException>().WithMessage("Email or password is incorrect.");

        _repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void CalculateAgeTest()
    {
        // Arrange
        var expectedAge = 35;
        var today = DateOnly.FromDateTime(DateTime.Now);
        var birthDate = today.AddYears(-expectedAge);

        if(today < birthDate.AddYears(expectedAge))
        {
            birthDate = birthDate.AddYears(-1);
        }

        var userId = Guid.NewGuid();
        var user = new User("Alice", "Baker", "alice@x", "p", birthDate)
        {
            Id = userId
        };

        _repoMock.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>())).Returns(user);

        // Act
        var age = _logic.CalculateAge(userId);

        // Assert
        Assert.AreEqual(expectedAge, age);
    }

    [TestMethod]
    public void GetOrThrow_Returns_User_When_Found()
    {
        // arrange
        var id = Guid.NewGuid();
        var user = new User("Alice", "Baker", "alice@x", "p", new DateOnly(1990, 1, 1)) { Id = id };

        _repoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((Expression<Func<User, bool>> pred) => pred.Compile()(user) ? user : null);

        // act
        var result = _logic.GetOrThrow(id);

        // assert
        result.Should().BeSameAs(user);
        _repoMock.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetOrThrow_Throws_When_NotFound()
    {
        // arrange
        var id = Guid.NewGuid();
        _repoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((User?)null);

        // act
        Action act = () => _logic.GetOrThrow(id);

        // assert
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage($"*{id}*");
        _repoMock.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void EditProfile_DoesNotUpdate_When_No_Fields_Are_Provided()
    {
        var id = Guid.NewGuid();
        var user = new User("Old", "User", "old@x", "oldp", new DateOnly(1990, 1, 1)) { Id = id };

        var repo = new Mock<IRepository<User>>(MockBehavior.Strict);
        repo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((Expression<Func<User, bool>> pred) => pred.Compile()(user) ? user : null);

        var logic = new UserLogic(repo.Object, _sessionLogic);

        logic.EditProfile(id, null, null, null, null, null);

        user.Name.Should().Be("Old");
        user.Surname.Should().Be("User");
        user.Email.Should().Be("old@x");
        user.Password.Should().Be("oldp");
        user.DateOfBirth.Should().Be(new DateOnly(1990, 1, 1));

        repo.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        repo.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        repo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void EditProfile_Trims_Email_And_Updates_User()
    {
        var id = Guid.NewGuid();
        var user = new User("Old", "User", "old@x", "oldp", new DateOnly(1990, 1, 1)) { Id = id };

        var repo = new Mock<IRepository<User>>(MockBehavior.Strict);
        repo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((Expression<Func<User, bool>> pred) => pred.Compile()(user) ? user : null);
        repo.Setup(r => r.Update(It.Is<User>(u => u.Id == id && u.Email == "new@x")))
            .Returns<User>(u => u);

        var logic = new UserLogic(repo.Object, _sessionLogic);

        logic.EditProfile(id, null, null, "  new@x  ", null, null);

        user.Email.Should().Be("new@x");
        repo.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Exactly(2));
        repo.Verify(r => r.Update(It.Is<User>(u => u.Id == id && u.Email == "new@x")), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void CalculateAge_Subtracts_One_When_Birthday_Has_Not_Occurred_Yet()
    {
        var userId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.Now);
        var birthDate = today.AddYears(-20).AddDays(1);

        var user = new User("Ana", "Perez", "ana@x", "p", birthDate) { Id = userId };

        _repoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((Expression<Func<User, bool>> pred) => pred.Compile()(user) ? user : null);

        var age = _logic.CalculateAge(userId);

        age.Should().Be(19);
        _repoMock.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _repoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Login_Trims_And_Normalizes_Email_Before_Searching()
    {
        var user = new User("Ana", "Perez", "ana@x", "p", new DateOnly(2000, 1, 1));

        _repoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((Expression<Func<User, bool>> pred) => pred.Compile()(user) ? user : null);

        _repoSessionMock
            .Setup(r => r.Add(It.IsAny<Session>()))
            .Returns((Session s) => s);

        var token = _logic.Login("  ANA@X  ", "p");

        token.Should().NotBeNull();
        _repoMock.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _repoSessionMock.Verify(r => r.Add(It.Is<Session>(s => s.UserId == user.Id)), Times.Once);
        _repoSessionMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void IsEmailUnique_Trims_And_Normalizes_Email()
    {
        var existing = new User("Ana", "Perez", "ana@x", "p", new DateOnly(2000, 1, 1));

        _repoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((Expression<Func<User, bool>> pred) => pred.Compile()(existing) ? existing : null);

        var result = _logic.IsEmailUnique("  ANA@X  ");

        result.Should().BeFalse();
        _repoMock.Verify(r => r.Find(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _repoMock.VerifyNoOtherCalls();
    }
}

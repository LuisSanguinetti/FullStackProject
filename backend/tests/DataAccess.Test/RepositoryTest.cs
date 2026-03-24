using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Test;

[TestClass]
public class RepositoryTest
{
    private ObligatorioDbContext? _context;
    private Repository<User>? _repository;
    private SqliteConnection? _connection;

    [TestInitialize]
    public void Setup()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ObligatorioDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ObligatorioDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new Repository<User>(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
        _connection?.Close();
    }

    [TestMethod]
    public void Add_ShouldAddUserToDatabase()
    {
        var u = new User("John", "Doe", "john@example.com", "password123", new DateOnly(2000, 1, 1));
        var result = _repository!.Add(u);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, _context!.Set<User>().Count());
        Assert.AreEqual("John", result.Name);
    }

    [TestMethod]
    public void Find_ShouldReturnUserWhenExists()
    {
        var u = new User("John", "Doe", "john@example.com", "password123", new DateOnly(2000, 1, 1));
        _context!.Set<User>().Add(u);
        _context.SaveChanges();

        var found = _repository!.Find(x => x.Email == "john@example.com");

        Assert.IsNotNull(found);
        Assert.AreEqual("John", found!.Name);
        Assert.AreEqual("Doe", found.Surname);
    }

    [TestMethod]
    public void Find_ShouldReturnNullWhenUserDoesNotExist()
    {
        var found = _repository!.Find(x => x.Email == "nope@example.com");
        Assert.IsNull(found);
    }

    [TestMethod]
    public void FindAll_ShouldReturnAllUsers()
    {
        _context!.Set<User>().AddRange(
            new User("Peter", "Parker", "peter@dailybugle.com", "sp1der", new DateOnly(1995, 8, 10)),
            new User("Clark", "Kent", "clark@dailyplanet.com", "krYpt0", new DateOnly(1980, 6, 18))
        );
        _context.SaveChanges();

        var all = _repository!.FindAll();

        Assert.AreEqual(2, all.Count);
        Assert.IsTrue(all.Any(x => x.Name == "Peter"));
        Assert.IsTrue(all.Any(x => x.Name == "Clark"));
    }

    [TestMethod]
    public void Update_ShouldModifyUser()
    {
        var u = new User("Steve", "Rogers", "steve@avengers.com", "shield", new DateOnly(1918, 7, 4));
        _context!.Set<User>().Add(u);
        _context.SaveChanges();

        u.Surname = "Romanova";
        var updated = _repository!.Update(u);

        Assert.IsNotNull(updated);
        Assert.AreEqual("Romanova", updated.Surname);
    }

    [TestMethod]
    public void Delete_ShouldRemoveUser()
    {
        var u = new User("Steve", "Rogers", "steve@avengers.com", "shield", new DateOnly(1918, 7, 4));
        _context!.Set<User>().Add(u);
        _context.SaveChanges();

        _repository!.Delete(u.Id);

        Assert.AreEqual(0, _context.Set<User>().Count());
    }

    [TestMethod]
    public void Add_ShouldReturnSameInstanceInserted()
    {
        var u = new User("Ted", "Lasso", "ted@football.org", "lasso", new DateOnly(1985, 8, 1));
        var result = _repository!.Add(u);

        Assert.IsTrue(object.ReferenceEquals(u, result));
    }

    [TestMethod]
    public void Delete_NonExisting_User_DoesNotThrow_CoversFalseBranch()
    {
        _repository!.Delete(Guid.NewGuid());
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void Find_UserRole_WithIncludes_LoadsUserAndRole()
    {
        var userRepo = new Repository<User>(_context!);
        var roleRepo = new Repository<Role>(_context!);
        var userRoleRepo = new Repository<UserRole>(_context!);

        var u = userRepo.Add(new User("Inc", "User", "inc@example.com", "pwd", new DateOnly(2001, 1, 1)));
        var r = roleRepo.Add(new Role("Admin"));

        _context!.Set<UserRole>().Add(new UserRole { UserId = u.Id, RoleId = r.Id, User = u, Role = r });
        _context.SaveChanges();

        var found = userRoleRepo.Find(x => x.UserId == u.Id && x.RoleId == r.Id, x => x.User, x => x.Role);
        Assert.IsNotNull(found);
        Assert.IsNotNull(found!.User);
        Assert.IsNotNull(found.Role);
    }

    [TestMethod]
    public void Find_UserRole_WithoutIncludes_AutoIncludesByReflection()
    {
        var userRepo = new Repository<User>(_context!);
        var roleRepo = new Repository<Role>(_context!);
        var userRoleRepo = new Repository<UserRole>(_context!);
        var user = new User("John", "Doe", "john@example.com", "password123", new DateOnly(2000, 1, 1));

        var u = userRepo.Add(user);
        var r = roleRepo.Add(new Role("User"));

        _context!.Set<UserRole>().Add(new UserRole { UserId = u.Id, RoleId = r.Id, User = u, Role = r });
        _context.SaveChanges();

        var found = userRoleRepo.Find(x => x.UserId == u.Id && x.RoleId == r.Id);
        Assert.IsNotNull(found);
        Assert.IsNotNull(found!.User);
        Assert.IsNotNull(found.Role);
    }

    [TestMethod]
    public void FindAll_UserRole_WithFilter_And_Includes()
    {
        var userRepo = new Repository<User>(_context!);
        var roleRepo = new Repository<Role>(_context!);
        var userRoleRepo = new Repository<UserRole>(_context!);

        var u = userRepo.Add(new User("All", "In", "allin@test.com", "pwd", new DateOnly(2002, 2, 2)));
        var r = roleRepo.Add(new Role("Member"));
        _context!.Set<UserRole>().Add(new UserRole { UserId = u.Id, RoleId = r.Id, User = u, Role = r });
        _context.SaveChanges();

        var list = userRoleRepo.FindAll(x => x.UserId == u.Id, x => x.User, x => x.Role);
        Assert.AreEqual(1, list.Count);
        Assert.IsNotNull(list[0].User);
        Assert.IsNotNull(list[0].Role);
    }

    [TestMethod]
    public void GetPage_ShouldReturnSecondPage_WithGivenOrder()
    {
        // Arrange
        _context!.Set<User>().AddRange(
            new User("U1", "A", "a@ex.com", "p1", new DateOnly(2000, 1, 1)),
            new User("U2", "B", "b@ex.com", "p2", new DateOnly(2000, 2, 1)),
            new User("U3", "C", "c@ex.com", "p3", new DateOnly(2000, 3, 1)),
            new User("U4", "D", "d@ex.com", "p4", new DateOnly(2000, 4, 1)),
            new User("U5", "E", "e@ex.com", "p5", new DateOnly(2000, 5, 1)),
            new User("U6", "F", "f@ex.com", "p6", new DateOnly(2000, 6, 1)),
            new User("U7", "G", "g@ex.com", "p7", new DateOnly(2000, 7, 1))
        );
        _context.SaveChanges();

        // Act
        var page = _repository!.GetPage(
            pageNumber: 2,
            pageSize: 3,
            filter: null,
            orderBy: q => q.OrderBy(u => u.Surname).ThenBy(u => u.Name)
        );

        // Assert
        Assert.AreEqual(3, page.Count, "Page size should be 3");
        var surnames = page.Select(u => u.Surname).ToList();
        CollectionAssert.AreEqual(new List<string> { "D", "E", "F" }, surnames);
    }

    [TestMethod]
    public void GetPage_UserRole_WithFilterAndIncludes_ReturnsFilteredFirstPage()
    {
        var userRepo = new Repository<User>(_context!);
        var roleRepo = new Repository<Role>(_context!);
        var userRoleRepo = new Repository<UserRole>(_context!);

        var r1 = roleRepo.Add(new Role("R1"));
        var r2 = roleRepo.Add(new Role("R2"));

        for (var i = 0; i < 5; i++)
        {
            _context!.Set<UserRole>().Add(new UserRole { User = userRepo.Add(new User($"U{i}", "A", $"u{i}@ex.com", "p", new DateOnly(2000,1,1))), Role = r1, RoleId = r1.Id });
        }

        for (var i = 0; i < 2; i++)
        {
            _context!.Set<UserRole>().Add(new UserRole { User = userRepo.Add(new User($"V{i}", "B", $"v{i}@ex.com", "p", new DateOnly(2000,1,1))), Role = r2, RoleId = r2.Id });
        }

        _context.SaveChanges();

        var page = userRoleRepo.GetPage(
            pageNumber: 1,
            pageSize: 2,
            filter: ur => ur.RoleId == r1.Id,
            orderBy: null,
            x => x.User, x => x.Role);

        Assert.AreEqual(2, page.Count);
        Assert.IsTrue(page.All(ur => ur.RoleId == r1.Id));
        Assert.IsTrue(page.All(ur => ur.User != null && ur.Role != null));
    }
}

using System.Linq.Expressions;
using Domain;
using IDataAccess;
using Moq;
using System.Linq.Expressions;

namespace Park.BusinessLogic.Test;

[TestClass]
public class ScoringStrategyMetaLogicTest
{
    private Mock<IRepository<ScoringStrategyMeta>> _repo = null!;
    private ScoringStrategyMetaLogic _logic = null!;

    [TestInitialize]
    public void Setup()
    {
        _repo = new Mock<IRepository<ScoringStrategyMeta>>(MockBehavior.Strict);
        _logic = new ScoringStrategyMetaLogic(_repo.Object);
    }

    [TestMethod]
    public async Task CreateFromUploadAsync_Happy()
    {
        ScoringStrategyMeta? captured = null;
        _repo.Setup(r => r.Add(It.IsAny<ScoringStrategyMeta>()))
             .Callback((ScoringStrategyMeta m) => captured = m)
             .Returns((ScoringStrategyMeta m) => m);

        var meta = await _logic.CreateFromUploadAsync("disp", "/p/a.dll", "a.dll");

        Assert.IsNotNull(captured);
        Assert.AreEqual(meta, captured);
        Assert.AreEqual("disp", meta.Name);
        Assert.AreEqual("/p/a.dll", meta.FilePath);
        Assert.AreEqual("a.dll", meta.FileName);
    }

    [TestMethod]
    public async Task ActivateAsync_Happy()
    {
        var id = Guid.NewGuid();
        var target = NewMeta(id, "t", active: false, deleted: false);
        var other = NewMeta(Guid.NewGuid(), "o", active: true, deleted: false);

        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>())).Returns(target);
        _repo.Setup(r => r.FindAll(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>())).Returns(new[] { other });
        _repo.Setup(r => r.Update(other)).Returns(other);
        _repo.Setup(r => r.Update(target)).Returns(target);

        await _logic.ActivateAsync(id);

        Assert.IsTrue(target.IsActive);
        Assert.IsTrue(other.IsActive);
    }

    [TestMethod]
    public async Task SoftDeleteAsync_Happy()
    {
        var id = Guid.NewGuid();
        var meta = NewMeta(id, "x", active: true, deleted: false);

        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>())).Returns(meta);
        _repo.Setup(r => r.Update(meta)).Returns(meta);

        await _logic.SoftDeleteAsync(id);

        Assert.IsTrue(meta.IsDeleted);
        Assert.IsFalse(meta.IsActive);
    }

    [TestMethod]
    public void GetActiveOrThrow_Happy()
    {
        var active = NewMeta(Guid.NewGuid(), "a", active: true, deleted: false);_repo.Setup(r => r.FindAll(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>()))
            .Returns(new List<ScoringStrategyMeta> { active });

        var got = _logic.GetActiveOrThrow();

        Assert.AreEqual(1, got.Count);
        Assert.AreSame(active, got[0]);
    }

    [TestMethod]
    public async Task UpdatePathAsync_Happy()
    {
        var id = Guid.NewGuid();
        var meta = NewMeta(id, "u", active: false, deleted: false);

        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>())).Returns(meta);
        _repo.Setup(r => r.Update(meta)).Returns(meta);

        await _logic.UpdatePathAsync(id, "/new/p.dll", "new.dll");

        Assert.AreEqual("/new/p.dll", meta.FilePath);
        Assert.AreEqual("new.dll", meta.FileName);
    }

    [TestMethod]
    public async Task ListAsync_Happy()
    {
        var a = NewMeta(Guid.NewGuid(), "a", false, false, DateTime.UtcNow.AddMinutes(-1));
        var b = NewMeta(Guid.NewGuid(), "b", false, false, DateTime.UtcNow);

        _repo.Setup(r => r.FindAll(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>())).Returns(new[] { a, b });

        var list = await _logic.ListAsync(false);

        Assert.AreEqual(2, list.Count);
        Assert.AreEqual(b.Id, list[0].Id);
        Assert.AreEqual(a.Id, list[1].Id);
    }

    private static ScoringStrategyMeta NewMeta(Guid id, string name, bool active, bool deleted, DateTime? created = null)
        => new ScoringStrategyMeta
        {
            Id = id,
            Name = name,
            FileName = $"{name}.dll",
            FilePath = $"/p/{name}.dll",
            IsActive = active,
            IsDeleted = deleted,
            CreatedOn = created ?? DateTime.UtcNow
        };

    [TestMethod]
    public void ActivateAsync_Idempotent_When_Target_Already_Active_And_No_Others()
    {
        var id = Guid.NewGuid();
        var target = new ScoringStrategyMeta { Id = id, Name = "t", CreatedOn = DateTime.UtcNow, FilePath = "t", FileName = "t.dll", IsActive = true, IsDeleted = false };
        var store = new List<ScoringStrategyMeta> { target };

        var repo = new Mock<IRepository<ScoringStrategyMeta>>(MockBehavior.Strict);
        repo.Setup(r => r.Find(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>()))
            .Returns((Expression<Func<ScoringStrategyMeta, bool>> p) => store.AsQueryable().FirstOrDefault(p));
        repo.Setup(r => r.FindAll(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>(), It.IsAny<Expression<Func<ScoringStrategyMeta, object>>[]>()))
            .Returns((Expression<Func<ScoringStrategyMeta, bool>> p, Expression<Func<ScoringStrategyMeta, object>>[] _) =>
                store.AsQueryable().Where(p).ToList());
        repo.Setup(r => r.Update(It.IsAny<ScoringStrategyMeta>())).Returns((ScoringStrategyMeta m) => m);

        var logic = new ScoringStrategyMetaLogic(repo.Object);

        logic.ActivateAsync(id).GetAwaiter().GetResult();

        Assert.IsTrue(target.IsActive);
        repo.Verify(r => r.Update(It.IsAny<ScoringStrategyMeta>()), Times.Once);
    }

    // runs with differet inputs

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("   ")]
    public void UpdatePathAsync_Updates_Path_Without_Changing_FileName_When_NullOrWhitespace(string? newName)
    {
        var id = Guid.NewGuid();
        var meta = new ScoringStrategyMeta { Id = id, Name = "n", CreatedOn = DateTime.UtcNow, FilePath = "old", FileName = "keep.dll", IsDeleted = false, IsActive = false };
        var store = new List<ScoringStrategyMeta> { meta };

        var repo = new Mock<IRepository<ScoringStrategyMeta>>(MockBehavior.Strict);
        repo.Setup(r => r.Find(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>()))
            .Returns((Expression<Func<ScoringStrategyMeta, bool>> p) => store.AsQueryable().FirstOrDefault(p));
        repo.Setup(r => r.Update(It.IsAny<ScoringStrategyMeta>())).Returns((ScoringStrategyMeta m) => m);

        var logic = new ScoringStrategyMetaLogic(repo.Object);

        logic.UpdatePathAsync(id, "C:\\new.dll", newName).GetAwaiter().GetResult();

        Assert.AreEqual("C:\\new.dll", meta.FilePath);
        Assert.AreEqual("keep.dll", meta.FileName);
        repo.Verify(r => r.Update(It.IsAny<ScoringStrategyMeta>()), Times.Once);
    }

    [TestMethod]
    public async Task CreateFromUploadAsync_Defaults_Name_When_DisplayName_Null()
    {
        ScoringStrategyMeta? captured = null;
        _repo.Setup(r => r.Add(It.IsAny<ScoringStrategyMeta>()))
             .Callback((ScoringStrategyMeta m) => captured = m)
             .Returns((ScoringStrategyMeta m) => m);

        var meta = await _logic.CreateFromUploadAsync(null, "/p/abc.dll", "abc.dll");

        Assert.IsNotNull(captured);
        Assert.AreSame(meta, captured);
        Assert.AreEqual("abc", meta.Name);
        Assert.IsFalse(meta.IsActive);
        Assert.IsFalse(meta.IsDeleted);
        Assert.AreNotEqual(default, meta.CreatedOn);
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task CreateFromUploadAsync_Throws_When_FilePath_Missing(string? badPath)
    {
        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _logic.CreateFromUploadAsync("x", badPath!, "a.dll"));
        StringAssert.Contains(ex.Message, "filePath");
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task CreateFromUploadAsync_Throws_When_FileName_Missing(string? badName)
    {
        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _logic.CreateFromUploadAsync("x", "/p/a.dll", badName!));
        StringAssert.Contains(ex.Message, "fileName");
    }

    [TestMethod]
    public async Task ActivateAsync_Throws_When_NotFound()
    {
        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>())).Returns((ScoringStrategyMeta?)null);

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _logic.ActivateAsync(Guid.NewGuid()));
    }

    [TestMethod]
    public async Task ActivateAsync_Throws_When_Deleted()
    {
        var meta = NewMeta(Guid.NewGuid(), "del", active: false, deleted: true);
        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>())).Returns(meta);

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _logic.ActivateAsync(meta.Id));
    }

    [TestMethod]
    public async Task SoftDeleteAsync_Throws_When_NotFound()
    {
        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>())).Returns((ScoringStrategyMeta?)null);

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _logic.SoftDeleteAsync(Guid.NewGuid()));
    }

    [TestMethod]
    public void GetActiveOrThrow_Throws_When_NoneActive()
    {
        _repo.Setup(r => r.FindAll(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>())).Returns(Array.Empty<ScoringStrategyMeta>());

        Assert.ThrowsException<InvalidOperationException>(() => _logic.GetActiveOrThrow());
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public async Task UpdatePathAsync_Throws_When_FilePath_Missing(string? badPath)
    {
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => _logic.UpdatePathAsync(Guid.NewGuid(), badPath!));
    }

    [TestMethod]
    public async Task UpdatePathAsync_Throws_When_NotFound()
    {
        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>())).Returns((ScoringStrategyMeta?)null);

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _logic.UpdatePathAsync(Guid.NewGuid(), "/p/new.dll"));
    }

    [TestMethod]
    public async Task ListAsync_Includes_Deleted_When_Requested_And_Orders_Newest_First()
    {
        var older = NewMeta(Guid.NewGuid(), "old", false, true, DateTime.UtcNow.AddMinutes(-5));
        var newer = NewMeta(Guid.NewGuid(), "new", true,  true, DateTime.UtcNow);

        _repo.Setup(r => r.FindAll(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>())).Returns(new[] { older, newer });

        var list = await _logic.ListAsync(includeDeleted: true);

        Assert.AreEqual(2, list.Count);
        Assert.AreEqual(newer.Id, list[0].Id);
        Assert.AreEqual(older.Id, list[1].Id);
    }

    [TestMethod]
    public void GetActiveOrThrowById_Happy()
    {
        var id = Guid.NewGuid();
        var store = new List<ScoringStrategyMeta>
        {
            NewMeta(Guid.NewGuid(), "x", active: true,  deleted: false),
            NewMeta(id,          "target", active: true,  deleted: false),
            NewMeta(Guid.NewGuid(), "y", active: false, deleted: false)
        };

        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>()))
             .Returns((Expression<Func<ScoringStrategyMeta, bool>> p) => store.AsQueryable().FirstOrDefault(p));

        var got = _logic.GetActiveOrThrowById(id);

        Assert.AreEqual(id, got.Id);
    }

    [TestMethod]
    public void GetActiveOrThrowById_Throws_When_NotFound_Or_NotActive()
    {
        var id = Guid.NewGuid();
        var store = new List<ScoringStrategyMeta>
        {
            NewMeta(Guid.NewGuid(), "a", active: true,  deleted: false),
            NewMeta(id,          "b", active: false, deleted: false),
            NewMeta(Guid.NewGuid(), "c", active: true,  deleted: true)
        };

        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>()))
             .Returns((Expression<Func<ScoringStrategyMeta, bool>> p) => store.AsQueryable().FirstOrDefault(p));

        Assert.ThrowsException<InvalidOperationException>(() => _logic.GetActiveOrThrowById(id));
    }
}

using System.Data.Common;
using Domain;
using IDataAccess;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public class ScoringStrategyMetaLogic : IScoringStrategyMetaLogic
{
    private readonly IRepository<ScoringStrategyMeta> _repo;

    public ScoringStrategyMetaLogic(IRepository<ScoringStrategyMeta> repo)
    {
        _repo = repo;
    }

    public Task<ScoringStrategyMeta> CreateFromUploadAsync(string? displayName, string filePath, string fileName)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("filePath is required.", nameof(filePath));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("fileName is required.", nameof(fileName));
        }

        var meta = new ScoringStrategyMeta
        {
            Id = Guid.NewGuid(),
            Name = displayName ?? Path.GetFileNameWithoutExtension(fileName),
            CreatedOn = DateTime.UtcNow,
            FilePath = filePath,
            FileName = fileName,
            IsActive = false,
            IsDeleted = false
        };

        _repo.Add(meta);
        return Task.FromResult(meta);
    }

    public Task ActivateAsync(Guid id)
    {
        var target = _repo.Find(m => m.Id == id);
        if (target is null)
        {
            // change exception? or add to the global exception handler
            throw new InvalidOperationException("Strategy not found.");
        }

        if (target.IsDeleted)
        {
            // change exception or add to the global exception handler
            throw new InvalidOperationException("Cannot activate a deleted strategy.");
        }

        target.IsActive = true;
        _repo.Update(target);

        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(Guid id)
    {
        var meta = _repo.Find(m => m.Id == id);
        if (meta is null)
        {
            throw new InvalidOperationException("Strategy not found.");
        }

        meta.IsDeleted = true;
        meta.IsActive = false;
        _repo.Update(meta);

        return Task.CompletedTask;
    }

    public List<ScoringStrategyMeta> GetActiveOrThrow()
    {
        var metas = _repo.FindAll(m => m.IsActive && !m.IsDeleted).ToList();
        if(metas.Count == 0)
        {
            throw new InvalidOperationException("No active scoring strategy configured.");
        }

        return metas;
    }

    public Task UpdatePathAsync(Guid id, string filePath, string? fileName = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("filePath is required.", nameof(filePath));
        }

        var meta = _repo.Find(m => m.Id == id)
                   ?? throw new InvalidOperationException("Strategy not found.");

        meta.FilePath = filePath;
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            meta.FileName = fileName;
        }

        _repo.Update(meta);
        return Task.CompletedTask;
    }

    public Task<List<ScoringStrategyMeta>> ListAsync(bool includeDeleted = false)
    {
        var items = _repo
            .FindAll(m => includeDeleted || !m.IsDeleted)
            .OrderByDescending(m => m.CreatedOn)
            .ToList();

        return Task.FromResult(items);
    }

    public ScoringStrategyMeta GetActiveOrThrowById(Guid strategyId)
    {
        var meta = _repo.Find(m => m.IsActive && !m.IsDeleted && m.Id == strategyId);
        if(meta == null)
        {
            throw new InvalidOperationException("No active scoring strategy configured.");
        }

        return meta;
    }
}

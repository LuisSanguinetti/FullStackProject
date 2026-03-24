using System.Linq.Expressions;

namespace IDataAccess;
public interface IRepository<T>
{
    T Add(T entity);
    T? Find(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includes);
    IList<T> FindAll(Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includes);
    T? Update(T entity);
    void Delete(Guid id);
    IList<T> GetPage(int pageNumber, int pageSize, Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, params Expression<Func<T, object>>[] includes);
}

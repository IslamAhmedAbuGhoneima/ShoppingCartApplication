using System.Linq.Expressions;

namespace ShoppingCart.Entities.Repositories
{
    public interface IGenericRepository<T> where T: class
    {

        IEnumerable<T> GetAll(string? includeWord = null);


        T Get(Expression<Func<T, bool>>? expression = null, string? includeWord = null);

        void Add(T entity);

        void Remove(T entity);

        void Update(T entity);

        void Save();

    }
}

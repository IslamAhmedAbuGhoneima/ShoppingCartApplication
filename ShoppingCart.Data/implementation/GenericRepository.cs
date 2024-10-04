using Microsoft.EntityFrameworkCore;
using ShoppingCart.DataAccess.Data;
using ShoppingCart.Entities.Repositories;
using System.Linq.Expressions;

namespace ShoppingCart.DataAccess.implementation
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        readonly AppDbContext context;
        DbSet<T> dbSet;

        public GenericRepository(AppDbContext _context)
        {
            context = _context;
            dbSet = context.Set<T>();
        }

        public void Add(T entity)
        {
            context.Add(entity);
        }

        public void Remove(T entity)
        {
            context.Remove(entity);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> expression, string? includeWord)
        {
            IQueryable<T> query = dbSet;

            if(expression != null)
            {
                query.Where(expression);
            }

            if(includeWord != null)
            {
                string[] includes = includeWord.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach(string include in includes)
                {
                    query = query.Include(include);
                }
            }

            return query.ToList();
        }

        

        public T Get(Expression<Func<T,bool>> expression,string? includeWord)
        {
            IQueryable<T> query = dbSet;
            
            if (includeWord != null)
            {
                string[] includes = includeWord.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (string include in includes)
                {
                    query = query.Include(include);
                }
            }
            return query.FirstOrDefault(expression);
        }

        public void Update(T entity)
        {
            context.Update(entity);
        }

        public void Save()
        {
            context.SaveChanges();
        }

    }
}

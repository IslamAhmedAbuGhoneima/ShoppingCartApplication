﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShoppingCart.DataAccess.Data;
using ShoppingCart.Entities.Repositories;
using System.Linq.Expressions;

namespace ShoppingCart.DataAccess.implementation
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext context;
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

        public IQueryable<T> GetAll(Expression<Func<T, bool>>? expression = null, string? includeWord = null)
        {
            IQueryable<T> query = dbSet;

            if(includeWord != null)
            {
                string[] includes = includeWord.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach(string include in includes)
                {
                    query = query.Include(include);
                }
            }

            if(expression is not null)
            {
                query = query.Where(expression);
            }

            return query;
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

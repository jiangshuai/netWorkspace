﻿// Copyright (c) love.net team. All rights reserved.

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Wedo.Vat.UnitOfWork {
    /// <summary>
    /// Represents a default generic repository implements the <see cref="IRepository{TEntity}"/> interface.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class {
        private readonly DbContext _dbContext;
        private readonly DbSet<TEntity> _dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public Repository(DbContext dbContext) {
            if (dbContext == null) {
                throw new ArgumentNullException("dbContext");
            }

            _dbContext = dbContext;
            _dbSet = _dbContext.Set<TEntity>();
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate. This method is no-tracking query.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An <see cref="IQueryable{TEntity}" /> that contains elements that satisfy the condition specified by predicate.</returns>
        /// <remarks>This method is no-tracking query.</remarks>
        public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate) {
            if (predicate == null)
            { 
                return _dbSet.AsNoTracking();
            }
            else {
                return _dbSet.AsNoTracking().Where(predicate);
            }
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate. This method will change tracking by context.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An <see cref="IQueryable{TEntity}" /> that contains elements that satisfy the condition specified by predicate.</returns>
        /// <remarks>This method will change tracking by context.</remarks>
        public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate) {
            if (predicate == null)
            {
                return _dbSet;
            }
            else
            {
                return _dbSet.Where(predicate);
            }
        }

        /// <summary>
        /// Uses raw SQL queries to fetch the specified <typeparamref name="TEntity" /> data.
        /// </summary>
        /// <param name="sql">The raw SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>An <see cref="IQueryable{TEntity}" /> that contains elements that satisfy the condition specified by raw SQL.</returns>
        public IQueryable<TEntity> FromSql(string sql, params object[] parameters) {
            return  _dbSet.SqlQuery(sql, parameters).AsQueryable();
        }

        /// <summary>
        /// Finds an entity with the given primary key values. If found, is attached to the context and returned. If no entity is found, then null is returned.
        /// </summary>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous insert operation.</returns>
        public TEntity Find(params object[] keyValues)
        {
            return  _dbSet.Find(keyValues);
        }

        /// <summary>
        /// Inserts a new entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <returns>A <see cref="Task{TEntity}" /> that represents the asynchronous insert operation.</returns>
        public void Insert(TEntity entity) {
             _dbSet.Add(entity); 
            // Shadow properties?
             var property = _dbContext.Entry(entity).Property("Created");
             if (property != null)
             {
                 property.CurrentValue = DateTime.Now;
             }
              _dbContext.SaveChanges();
        }

        /// <summary>
        /// Inserts a range of entities asynchronously.
        /// </summary>
        /// <param name="entities">The entities to insert.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous insert operation.</returns>
        public void Insert(params TEntity[] entities)
        {
            _dbSet.AddRange(entities);
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Inserts a range of entities asynchronously.
        /// </summary>
        /// <param name="entities">The entities to insert.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous insert operation.</returns>
        public void Insert(IEnumerable<TEntity> entities)
        {
            _dbSet.AddRange(entities);
            _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Update(TEntity entity) { 
            
            var property = _dbContext.Entry(entity).Property("ModifiedTime");
            if(property != null) {
                property.CurrentValue = DateTime.Now;
            }
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Updates the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void Update(params TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                var property = _dbContext.Entry(entity).Property("ModifiedTime");
                if (property != null)
                {
                    property.CurrentValue = DateTime.Now;
                }
            } 
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Updates the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void Update(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                var property = _dbContext.Entry(entity).Property("ModifiedTime");
                if (property != null)
                {
                    property.CurrentValue = DateTime.Now;
                }
            }
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        public void Delete(TEntity entity) {
            _dbSet.Remove(entity);
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Deletes the entity by the specified primary key.
        /// </summary>
        /// <param name="id">The primary key value.</param>
        public void Delete(object id) {
            // using a stub entity to mark for deletion
            var typeInfo = typeof(TEntity).GetTypeInfo();
            // REVIEW: using metedata to find the key rather than use hardcode 'id'
            var property = typeInfo.GetProperty("Id");
            if (property != null) {
                var entity = Activator.CreateInstance<TEntity>();
                property.SetValue(entity, id);
                _dbContext.Entry(entity).State = EntityState.Deleted;
            }
            else {
                var entity = _dbSet.Find(id);
                if (entity != null) {
                    Delete(entity);
                }
            }
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Deletes the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void Delete(params TEntity[] entities) {
            _dbSet.RemoveRange(entities);
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Deletes the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public void Delete(IEnumerable<TEntity> entities) {
            _dbSet.RemoveRange(entities);
            _dbContext.SaveChanges();
        }


        public bool Exist(Expression<Func<TEntity, bool>> predicate)
        {
           return _dbSet.Where(predicate).Count()>0;
        }

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.FirstOrDefault(predicate);
        } 
    }
}

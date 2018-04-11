using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Pikachu.Core.Repository.Sql.Interfaces;

namespace Pikachu.Core.Repository.Sql
{
    public class UnitOfWork : IDisposable, IUnitOfWork
    {
        #region Private Fields
        private readonly IDbContext _context;
        private Dictionary<string, dynamic> _repositories;
        private bool _disposed;
        #endregion Private Fields

        public Guid InstanceId { get; }

        #region Constuctor/Dispose
        public UnitOfWork(IDbContext context)
        {
            _context = context;
            InstanceId = Guid.NewGuid();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IDbContextTransaction CreateNewContextTransaction(IsolationLevel isolationLevel)
        {
            return ((DbContext) _context).Database.BeginTransaction(isolationLevel);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
        #endregion Constuctor/Dispose

        public void Save()
        {
            _context.SaveChanges();
        }

        public Task<int> SaveAsync()
        {
            return _context.SaveChangesAsync();
        }

        public Task<int> SaveAsync(CancellationToken cancellationToken)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : class, new()
        {
            if (_repositories == null)
            {
                _repositories = new Dictionary<string, dynamic>();
            }

            var type = typeof(TEntity).Name;

            if (_repositories.ContainsKey(type))
            {
                return (IRepository<TEntity>)_repositories[type];
            }

            var repositoryType = typeof(Repository<>);
            _repositories.Add(type, Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _context));

            return (IRepository<TEntity>)_repositories[type];
        }

        public IEnumerable<ValidationResult> GetValidationErrors()
        {
            return _context.GetValidationErrors();
        }
    }
}
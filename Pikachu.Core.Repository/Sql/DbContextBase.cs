using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pikachu.Core.Repository.Helpers;
using Pikachu.Core.Repository.Sql.Interfaces;

namespace Pikachu.Core.Repository.Sql
{
    public class DbContextBase : DbContext, IDbContext
    {
        public DbContextBase(DbContextOptions options)
            : base(options)
        {
            InstanceId = Guid.NewGuid();
        }

        public Guid InstanceId { get; }

        public void ApplyStateChanges()
        {
            foreach (var dbEntityEntry in ChangeTracker.Entries())
            {
                var entityState = dbEntityEntry.Entity as IObjectState;
                if (entityState == null)
                    throw new InvalidCastException("All entites must implement the IObjectState interface, " +
                                                   "this interface must be implemented so each entites state can explicitely determined when updating graphs.");
  
                dbEntityEntry.State = GenericHelpers.ConvertState(entityState.State);
            }
        }

        public override int SaveChanges()
        {
            ApplyStateChanges();
            return base.SaveChanges();
        }

        public Task<int> SaveChangesAsync()
        {
            ApplyStateChanges();
            return base.SaveChangesAsync();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            ApplyStateChanges();
            return base.SaveChangesAsync(cancellationToken);
        }

        public IEnumerable<ValidationResult> GetValidationErrors()
        {
            return ChangeTracker
                .Entries<IValidatableObject>()
                .SelectMany(e => e.Entity.Validate(null))
                .Where(r => r != ValidationResult.Success);
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace Pikachu.Core.Repository.Sql.Interfaces
{
    public interface IDbContext
    {
        Guid InstanceId { get; }
        //IConfigurationRoot GetConfiguration(string appSettingsFileName);
        DbSet<T> Set<T>() where T : class;
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task<int> SaveChangesAsync();
        void Dispose();
        IEnumerable<ValidationResult> GetValidationErrors();
        void ApplyStateChanges();

    }
}
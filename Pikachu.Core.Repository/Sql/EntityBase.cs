using System.ComponentModel.DataAnnotations.Schema;
using Pikachu.Core.Repository.Helpers;
using Pikachu.Core.Repository.Sql.Interfaces;

namespace Pikachu.Core.Repository.Sql
{
    public abstract class EntityBase : IObjectState
    {
        [NotMapped]
        public ObjectState State { get; set; }
    }
}
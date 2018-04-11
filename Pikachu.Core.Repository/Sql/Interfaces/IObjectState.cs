using System.ComponentModel.DataAnnotations.Schema;
using Pikachu.Core.Repository.Helpers;

namespace Pikachu.Core.Repository.Sql.Interfaces
{
    public interface IObjectState
    {
        [NotMapped]
        ObjectState State { get; set; }
    }
}
using System.Collections.Generic;
using Pikachu.Core.Repository.Sql;

namespace Helpdesk.Data.Models
{
    public partial class RequestType : EntityBase
    {
        public RequestType()
        {
            Request = new HashSet<Request>();
        }

        public byte Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Request> Request { get; set; }
    }
}

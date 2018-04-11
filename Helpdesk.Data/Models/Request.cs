using System;
using System.Collections.Generic;
using Pikachu.Core.Repository.Sql;

namespace Helpdesk.Data.Models
{
    public partial class Request : EntityBase
    {
        public Request()
        {
            Response = new HashSet<Response>();
        }

        public Guid Id { get; set; }
        public byte RequestTypeId { get; set; }
        public string Header { get; set; }
        public string Body { get; set; }
        public Guid UserId { get; set; }

        public virtual ICollection<Response> Response { get; set; }
        public virtual RequestType RequestType { get; set; }
        public virtual User User { get; set; }
    }
}

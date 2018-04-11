using System;
using Pikachu.Core.Repository.Sql;

namespace Helpdesk.Data.Models
{
    public partial class Response : EntityBase
    {
        public Guid Id { get; set; }
        public Guid RequestId { get; set; }
        public string Header { get; set; }
        public string Body { get; set; }
        public Guid UserId { get; set; }

        public virtual Request Request { get; set; }
        public virtual User User { get; set; }
    }
}

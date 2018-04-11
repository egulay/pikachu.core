using System;
using System.Collections.Generic;
using Pikachu.Core.Repository.Sql;

namespace Helpdesk.Data.Models
{
    public partial class User : EntityBase
    {
        public User()
        {
            Request = new HashSet<Request>();
            Response = new HashSet<Response>();
        }

        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<Request> Request { get; set; }
        public virtual ICollection<Response> Response { get; set; }
    }
}

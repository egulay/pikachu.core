using System.Collections.Generic;
using System.Linq;
using Helpdesk.Data.Models;
using Pikachu.Core.Repository.Sql.Interfaces;

namespace Helpdesk.Data.Extensions
{
    public static class UserExtension
    {
        public static long GetCount(this IRepository<User> userRepository)
        {
            return userRepository.Query().Get().Count();
        }

        public static IEnumerable<User> GetActiveUsers
            (this IRepository<User> userRepository)
        {
            return userRepository.Query().Filter(u => u.IsActive).Get();
        }
    }
}

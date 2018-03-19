using AlmostAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Repositories
{
    public class MainRepository
    {
        private ApplicationContext _applicationContext;

        public MainRepository(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        internal User GetUserByEmail(string userEmail)
        {
            return _applicationContext.Users.FirstOrDefault(u => u.Email == userEmail);
        }
    }
}

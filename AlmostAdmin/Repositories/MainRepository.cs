using AlmostAdmin.Models;
using Microsoft.EntityFrameworkCore;
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

        internal Project GetProjectById(int projectId)
        {
            var project = _applicationContext.Projects
                .Include(p => p.UserProjects)
                    .ThenInclude(up => up.User)
                .Include(p => p.Questions)
                    .ThenInclude(q => q.Answer)
                .Include(p => p.Questions)
                    .ThenInclude(q => q.QuestionTags)
                        .ThenInclude(qt => qt.Tag)
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return null;
            
            return project;
        }
    }
}

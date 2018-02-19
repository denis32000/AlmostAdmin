using AlmostAdmin.Data.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Models
{
    public class User : IdentityUser
    {
        public AdminLevels AdminLevel { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }
    }
}

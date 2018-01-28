using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Models
{
    public class User : IdentityUser
    {
        public int AdminLevel { get; set; }
    }
}

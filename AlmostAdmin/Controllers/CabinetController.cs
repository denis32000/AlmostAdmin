using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlmostAdmin.Models;
using AlmostAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlmostAdmin.Controllers
{
    [Authorize]
    public class CabinetController : Controller
    {
        private MainService _mainService;
        private ApplicationContext _applicationContext;

        public CabinetController(MainService mainService, ApplicationContext applicationContext)
        {
            _mainService = mainService;
            _applicationContext = applicationContext;
        }

        public IActionResult Index()
        {
            var user = _mainService.GetUserFromClaims(User);

            if (user == null)
                return View("Error");

            var projects = _applicationContext.Projects
                .Include(p => p.UserProjects)
                    .ThenInclude(up => up.User)
                .Where(p => p.UserProjects.FirstOrDefault(up => up.UserId == user.Id) != null)
                .ToList();

            return View(projects);
        }
        
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlmostAdmin.Models;
using AlmostAdmin.Services;
using AlmostAdmin.ViewModels;
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
                return RedirectToAction("Error", "Home");

            var projects = _applicationContext.Projects
                .Include(p => p.UserProjects)
                    .ThenInclude(up => up.User).ToList();
                //.Where(p => p.UserProjects.FirstOrDefault(up => up.UserId == user.Id) != null)
                //.ToList();

            if (projects.Any())
            {
                return View(projects);
            }

            return RedirectToAction("CreateProject");
        }
        
        public IActionResult CreateProject()
        {
            return View("Create");
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject(ProjectViewModel projectModel)
        {
            var user = _mainService.GetUserFromClaims(User);

            // TODO: use service instead
            var userProject = new UserProject
            {
                User = user
            };

            var project = new Project
            {
                Name = projectModel.Name,
                UserProjects = new List<UserProject> { userProject }
            };
            
            _applicationContext.Projects.Add(project);
            await _applicationContext.SaveChangesAsync();

            return RedirectToAction("Index", "Cabinet");
        }
    }
}
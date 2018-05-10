using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AlmostAdmin.Models;
using AlmostAdmin.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace AlmostAdmin.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        // TODO: FOR TESTING PURPOSE, MOVE THIS METHODS TO REPOSITORY
        ////////////////////////////////////////////////////////
        private ApplicationContext _applicationContext;

        public HomeController(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }
        ////////////////////////////////////////////////////////

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult TestStatusUrl([FromForm]string data, [FromForm]string signature)
        {
            return Json("*OK*");
        }

        public IActionResult Init()
        {
            // TODO: reset all answers. Debug command
            //var project
            //_applicationContext.Projects.Add();
            return View();
        }

        public IActionResult Reset()
        {
            // TODO: reset all answers. Debug command
            return View();
        }

        public IActionResult ApiDocs()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            var viewModel = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
            return View(viewModel);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlmostAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlmostAdmin.Controllers
{
    [Authorize]
    public class PanelController : Controller
    {
        private MainService _mainService;

        public PanelController(MainService mainService)
        {
            _mainService = mainService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Questions()
        {
            var listOfQuestions = _mainService.GetListOfQuestions();
            return View(listOfQuestions);
        }

        public IActionResult Answers()
        {
            var listOfQuestions = _mainService.GetListOfAnswers();
            return View(listOfQuestions);
        }
    }
}
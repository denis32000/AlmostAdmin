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
    public class PanelController : Controller
    {
        private MainService _mainService;
        private ApplicationContext _applicationContext;

        public PanelController(MainService mainService, ApplicationContext applicationContext)
        {
            _mainService = mainService;
            _applicationContext = applicationContext;
        }

        public IActionResult Index(int projectId)
        {
            var project = _applicationContext.Projects
                .Include(p => p.Questions)
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return RedirectToAction("Error", "Home");
            
            return View(project);
        }

        [HttpGet]
        public IActionResult Answer()
        {
            var listOfQuestions = _mainService.GetListOfAnswers();
            return View(listOfQuestions);
        }

        public async Task<bool> CreateQuestion(int projectId, string questionText)// TODO: , ICollection<string> tags)
        {
            if (string.IsNullOrEmpty(questionText))
                return false;

            var project = _applicationContext.Projects.FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return false;

            var questionTags = new List<QuestionTag>();
            
            var question = new Question
            {
                Text = questionText,
                Date = DateTime.Now,
                Project = project,
                QuestionTags = questionTags
            };

            _applicationContext.Questions.Add(question);
            await _applicationContext.SaveChangesAsync();

            return true;
        }


        [HttpGet]
        public IActionResult Questions()
        {
            var listOfQuestions = _mainService.GetListOfQuestions();
            return View(listOfQuestions);
        }

        [HttpGet]
        public IActionResult Answers()
        {
            var listOfQuestions = _mainService.GetListOfAnswers();
            return View(listOfQuestions);
        }
    }
}
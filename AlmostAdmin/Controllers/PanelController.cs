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
                //.Include(p => p.Questions)
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return RedirectToAction("Error", "Home");

            project.Questions = _applicationContext.Questions
                .Include(q => q.Answer)
                .Include(q => q.QuestionTags)
                .Where(q => q.Project.Id == projectId)
                .ToList();
            
            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> Answer(int projectId, int questionId, string answerText)
        {
            if (string.IsNullOrEmpty(answerText))
                return BadRequest();

            var project = _applicationContext.Projects
                .Include(p => p.Questions)
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return BadRequest();

            var answer = new Answer
            {
                Date = DateTime.Now,
                Text = answerText,
                Project = project
            };

            var question = project.Questions.FirstOrDefault(q => q.Id == questionId);

            if (question == null)
                return BadRequest();

            question.Answer = answer;
            question.AnsweredByHuman = true;

            _applicationContext.Answers.Add(answer);
            await _applicationContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuestion(int projectId, string questionText)// TODO: , ICollection<string> tags)
        {
            if (string.IsNullOrEmpty(questionText))
                return BadRequest();

            var project = _applicationContext.Projects
                .Include(p => p.Questions)
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return BadRequest();

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

            return Ok();
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
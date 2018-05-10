using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlmostAdmin.Models;
using AlmostAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlmostAdmin.Controllers
{
    public class Result
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    [Authorize]
    public class PanelController : Controller
    {
        private MainService _mainService;
        private ApplicationContext _applicationContext;
        private Project _project;
        private ProcessorService _processorService;
        private IHostingEnvironment _he;

        public PanelController(MainService mainService, ApplicationContext applicationContext, ProcessorService processorService, IHostingEnvironment he)
        {
            _mainService = mainService;
            _applicationContext = applicationContext;
            _processorService = processorService;
            _he = he;
        }

        public IActionResult Index(int projectId)
        {
            _project = _mainService.GetProjectById(projectId);

            if (_project == null)
                return RedirectToAction("Error", "Home");

            
            return View(_project);
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

            var user = _mainService.GetUserByClaims(User);

            if (user == null)
                return BadRequest();

            var answer = new Answer
            {
                Date = DateTime.Now,
                Text = answerText,
                Project = project,
                User = user
            };

            var question = project.Questions.FirstOrDefault(q => q.Id == questionId);

            if (question == null)
                return BadRequest();

            question.Answer = answer;
            question.AnsweredByHuman = true;

            _applicationContext.Answers.Add(answer);
            await _applicationContext.SaveChangesAsync();

            _processorService.AnswerOnSimilarQuestionsAsync(questionId);
            //_processorService.TrySendQuestionAnswerAsync(questionId);
            
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
                QuestionTags = questionTags,
                StatusUrl = Url.Action("TestStatusUrl", "Home", null, Request.Scheme) // TODO: test purpose
            };

            _applicationContext.Questions.Add(question);
            await _applicationContext.SaveChangesAsync();

            // Запускаем обработку вопроса без ожидания результата
            _processorService.FindAnswersForQuestionAsync(question.Id, _he.ContentRootPath);

            return Ok();
        }

        [HttpPost]
        public async Task<JsonResult> InviteUser(int projectId, string emailToInvite)
        {
            if (string.IsNullOrEmpty(emailToInvite))
                return Json(new Result { Message = "Parameter emailToInvite" });

            var project = _applicationContext.Projects
                .Include(p => p.Questions)
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return Json(new Result { Message = "Parameter projectId" });

            var user = _mainService.GetUserByEmail(emailToInvite);
            if (user == null)
                return Json(new Result { Message = "User with such email doesn't exist." });

            var userProject = new UserProject
            {
                Project = project,
                User = user
            };

            project.UserProjects.Add(userProject);
            await _applicationContext.SaveChangesAsync();

            return Json(new Result { Success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ActivateFastAnswers(int projectId)
        {
            var project = _applicationContext.Projects
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return Json(new Result { Message = "Parameter projectId" });

            project.AnswerWithoutApprove = true;
            await _applicationContext.SaveChangesAsync();

            return Json(new Result { Success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateFastAnswers(int projectId)
        {
            var project = _applicationContext.Projects
                .FirstOrDefault(p => p.Id == projectId);

            if(project == null)
                return Json(new Result { Message = "Parameter projectId" });

            project.AnswerWithoutApprove = false;
            await _applicationContext.SaveChangesAsync();

            return Json(new Result { Success = true });
        }
        //public IActionResult ProjectUsers()
        //{
        //    return PartialView("", _project);
        //}

        //[HttpGet]
        //public IActionResult Questions()
        //{
        //    var listOfQuestions = _mainService.GetListOfQuestions();
        //    return View(listOfQuestions);
        //}

        //[HttpGet]
        //public IActionResult Answers()
        //{
        //    var listOfQuestions = _mainService.GetListOfAnswers();
        //    return View(listOfQuestions);
        //}
    }
}
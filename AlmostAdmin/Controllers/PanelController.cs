using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlmostAdmin.Models;
using AlmostAdmin.Services;
using AlmostAdmin.ViewModels;
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
        private RepositoryService _repositoryService;
        private ApplicationContext _applicationContext;
        private Project _project;
        private ProcessorService _processorService;

        public PanelController(RepositoryService repositoryService, ApplicationContext applicationContext, ProcessorService processorService)
        {
            _repositoryService = repositoryService;
            _applicationContext = applicationContext;
            _processorService = processorService;
        }

        public IActionResult Index(int projectId)
        {
            _project = _repositoryService.GetProjectById(projectId);

            if (_project == null)
                return RedirectToAction("Error", "Home");

            
            return View(_project);
        }

        [HttpPost]
        public async Task<IActionResult> Answer(int projectId, int questionId, string answerText)
        {
            if (string.IsNullOrEmpty(answerText))
                return Json(new Result { Message = "Текст ответа не может быть пустым." });

            var project = _applicationContext.Projects
                .Include(p => p.Questions)
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return Json(new Result { Message = "Parameter projectId" });

            var user = _repositoryService.GetUserByClaims(User);

            if (user == null)
                return Json(new Result { Message = "Ошибка с идентификацией пользователя." });

            var answer = new Answer
            {
                Date = DateTime.Now,
                Text = answerText,
                Project = project,
                User = user
            };

            var question = project.Questions.FirstOrDefault(q => q.Id == questionId);

            if (question == null)
                return Json(new Result { Message = "Parameter questionId" });

            question.Answer = answer;
            question.AnsweredByHuman = true;

            _applicationContext.Answers.Add(answer);
            await _applicationContext.SaveChangesAsync();

            await _processorService.TrySendQuestionAnswerAsync(questionId);
            _processorService.AnswerOnSimilarQuestionsAsync(questionId);

            return Json(new Result { Success = true });
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuestion(int projectId, string questionText)// TODO: , ICollection<string> tags)
        {
            if (string.IsNullOrEmpty(questionText))
                return Json(new Result { Message = "Текст вопроса не может быть пустым." });

            var project = _applicationContext.Projects
                .Include(p => p.Questions)
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return Json(new Result { Message = "Parameter projectId" });

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
            await _processorService.FindAnswersForQuestionAsync(question.Id);

            return Json(new Result { Success = true });
        }
        
        public async Task<ActionResult> GetProjectQuestions(int projectId, int page = 1, int priority = 0, string text = "")
        {
            try
            {
                int pageSize = 10;   // количество элементов на странице

                IQueryable<Question> source = _applicationContext.Questions
                    .Include(q => q.Answer)
                    .Include(q => q.QuestionTags)
                        .ThenInclude(qt => qt.Tag)
                    .Where(q => q.ProjectId == projectId);

                if (!string.IsNullOrEmpty(text))
                {
                    var keyWords = text.Split(',');
                    foreach (var keyword in keyWords)
                    {
                        if (string.IsNullOrEmpty(keyword))
                            continue;

                        source = source.Where(q => q.Text.Contains(keyword) || q.Answer.Text.Contains(keyword));
                    }
                }

                // priority: 0-all, 1-system, 2-empty, 3-human
                switch (priority)
                {
                    case 0:// all
                        {
                            source = source
                                .OrderByDescending(q => !q.HasApprovedAnswer) // ответы с человеком в самом конце
                                .ThenByDescending(q => q.Answer != null)// поднимаем в самый верх те, у которых ЕСТЬ ответ системой
                                .ThenByDescending(q => q.Date); 
                            break;
                        }
                    case 1:// system
                        {
                            source = source.Where(q => q.Answer != null && !q.HasApprovedAnswer)
                                .OrderByDescending(q => q.Date);
                            break;
                        }
                    case 2:// empty
                        {
                            source = source.Where(q => q.Answer == null)
                                .OrderByDescending(q => q.Date);
                            break;
                        }
                    case 3:// human
                        {
                            source = source.Where(q => q.Answer != null && q.HasApprovedAnswer)
                                .OrderByDescending(q => q.Date);
                            break;
                        }
                }

                var count = await source.CountAsync();
                var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                var pageViewModel = new PageViewModel(count, page, pageSize);
                var viewModel = new QuestionsViewModel
                {
                    PageViewModel = pageViewModel,
                    Questions = items
                };

                return PartialView("_GetProjectQuestions", viewModel);
            }
            catch (Exception ex)
            {
                return Content("Ошибка поиска данных.<br\\>" + ex.Message + ex.InnerException?.Message);
            }
        }

        public ActionResult GetSimilar(int questionId)
        {
            var question = _applicationContext.Questions
                .Include(q => q.Answer)
                .FirstOrDefault(q => q.Id == questionId);

            if (question == null)
                return Content("Ошибка: неверный ID вопроса.");

            var ids = _processorService.GetListOfSimilarQuestionIds(question.Text, question.ProjectId);

            var similarQuestions = _applicationContext.Questions
                .Include(q => q.Answer)
                .Where(q => ids.Contains(q.Id) && q.Id != questionId)
                .OrderByDescending(q => q.Answer != null)
                .ToList();

            var viewModel = new SimilarQuestionsViewModel
            {
                QuestionId = questionId,
                SimilarQuestions = similarQuestions
            };

            return PartialView("_SimilarQuestions", viewModel);
        }

        public async Task<ActionResult> Approve(int questionId, int answerId)
        {
            var question = _applicationContext.Questions.FirstOrDefault(q => q.Id == questionId);

            if (question == null)
                return Json(new Result { Message = "Parameter questionId" });

            var answer = _applicationContext.Answers.FirstOrDefault(a => a.Id == answerId);

            if (answer == null)
                return Json(new Result { Message = "Parameter answerId" });

            question.Answer = answer;
            question.ApprovedByHuman = true;
            await _applicationContext.SaveChangesAsync();

            await _processorService.TrySendQuestionAnswerAsync(questionId);
            _processorService.AnswerOnSimilarQuestionsAsync(questionId);

            return Json(new Result { Success = true });
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

            var user = _repositoryService.GetUserByEmail(emailToInvite);
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
        public async Task<IActionResult> SwitchFastAnswers(int projectId)
        {
            var project = _applicationContext.Projects
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return Json(new Result { Message = "Parameter projectId" });

            project.AnswerWithoutApprove = !project.AnswerWithoutApprove;
            await _applicationContext.SaveChangesAsync();

            return Json(new Result { Success = true });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveProjectMember(int projectId, string userId)
        {
            var project = _applicationContext.Projects
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return Json(new Result { Message = "Parameter projectId" });

            var user = _applicationContext.Users
                .Include(u => u.UserProjects)
                .FirstOrDefault(p => p.Id == userId);

            if (user == null)
                return Json(new Result { Message = "Parameter userId" });

            var userProject = user.UserProjects.FirstOrDefault(up => up.ProjectId == projectId);
            user.UserProjects.Remove(userProject);
            await _applicationContext.SaveChangesAsync();

            return Json(new Result { Success = true });
        }

        [HttpPost]
        public async Task<IActionResult> AddQuestionTag(int projectId, int questionId, string tagText)
        {
            if (string.IsNullOrEmpty(tagText))
                return Json(new Result { Message = "Parameter tagText" });

            var project = _applicationContext.Projects
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return Json(new Result { Message = "Parameter projectId" });
            
            var question = _applicationContext.Questions
                .Include(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
                .FirstOrDefault(q => q.Id == questionId);

            if (question == null)
                return Json(new Result { Message = "Parameter questionId" });

            if(question.QuestionTags.FirstOrDefault(qt => qt.Tag.Text == tagText) != null)
                return Json(new Result { Message = "Такой тэг уже привязан к этому вопросу!" });

            var newTag = new Tag
            {
                Text = tagText
            };

            var newQuestionTag = new QuestionTag
            {
                Question = question,
                Tag = newTag
            };

            question.QuestionTags.Add(newQuestionTag);
            await _applicationContext.SaveChangesAsync();

            return Json(new Result { Success = true });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveQuestionTag(int projectId, int questionId, int tagId)
        {
            var project = _applicationContext.Projects
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return Json(new Result { Message = "Parameter projectId" });

            var question = _applicationContext.Questions
                .Include(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
                .FirstOrDefault(q => q.Id == questionId);

            if (question == null)
                return Json(new Result { Message = "Parameter questionId" });

            var questionTag = question.QuestionTags.FirstOrDefault(qt => qt.TagId == tagId);

            if(questionTag == null)
                return Json(new Result { Message = "Parameter tagId" });

            question.QuestionTags.Remove(questionTag);
            await _applicationContext.SaveChangesAsync();

            return Json(new Result { Success = true });
        }

        public async Task<IActionResult> DeleteQuestion(int projectId, int questionId)
        {
            var project = _applicationContext.Projects
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
                return Json(new Result { Message = "Parameter projectId" });

            var question = _applicationContext.Questions
                .FirstOrDefault(q => q.Id == questionId);

            if (question == null)
                return Json(new Result { Message = "Parameter questionId" });

            _applicationContext.Questions.Remove(question);
            await _applicationContext.SaveChangesAsync();

            return Json(new Result { Success = true });
        }

        public IActionResult DeleteProject(int projectId)
        {
            var proj = _applicationContext.Projects.FirstOrDefault(p => p.Id == projectId);
            _applicationContext.Projects.Remove(proj);

            return Ok();
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
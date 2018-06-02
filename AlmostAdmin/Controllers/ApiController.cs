using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlmostAdmin.Common;
using AlmostAdmin.Models;
using AlmostAdmin.Models.Api;
using AlmostAdmin.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AlmostAdmin.Controllers
{
    /*
    public class QuestionPostModel
    {
        public string data { get; set; }
        public string signature { get; set; }
    }
    */

    [Route("api/[controller]")]
    public class QuestionController : Controller
    {
        // TODO: возможность присылать ответ на вопрос указывая ИД вопроса
        // TODO: возможность запросить ответ на вопрос по айдишнику вопроса
        // TODO: возможность получить айдишник вопроса по полному тексту вопроса 1 в 1

        //private MainService _mainService;
        private ApplicationContext _applicationContext;
        private ProcessorService _processorService;

        public QuestionController(RepositoryService mainService, ApplicationContext applicationContext, ProcessorService processorService)
        {
            //_mainService = mainService;
            _applicationContext = applicationContext;
            _processorService = processorService;
        }

        [HttpPost]
        public async Task<JsonResult> Post([FromForm] string data, [FromForm]string signature)
        {
            try
            {
                //string answerJson;
                var decodedData = CryptoUtils.Base64Decode(data);
                var questionToApi = JsonConvert.DeserializeObject<QuestionToApi>(decodedData);

                if (!questionToApi.IsModelValid())
                {
                    throw new ErrorWithDataException("Some of the data parameters are invalid. Check the documentation.",
                        Models.Api.StatusCode.WrongData);
                }

                if (!Utils.ValidUrl(questionToApi.StatusUrl))
                {
                    throw new ErrorWithDataException("Provided URL is not valid.",
                        Models.Api.StatusCode.WrongStatusUrl);
                }

                var user = _applicationContext.User
                    .Include(u => u.UserProjects)
                        .ThenInclude(up => up.Project)
                    .FirstOrDefault(u => u.UserName == questionToApi.Login);

                if (user == null)// || user.PasswordHash != questionToApi.PasswordHash)
                {
                    throw new ErrorWithDataException("User with such email doesn't exist.",
                        Models.Api.StatusCode.WrongLoginPasswordCredentials);
                }

                var userProject = user.UserProjects.FirstOrDefault(up => up.Project.Id == questionToApi.ProjectId);

                if (userProject == null)
                {
                    throw new ErrorWithDataException("Provided user doesn't consist in the project with such ID.",
                        Models.Api.StatusCode.WrongProjectId);
                }

                if (!Utils.ValidateSignature(data, signature, userProject.Project.PrivateKey))
                {
                    throw new ErrorWithDataException("Signature is not valid. Check your PrivateKey and MD5 alghorithm.",
                        Models.Api.StatusCode.WrongSignature);
                }

                var question = new Question
                {
                    Date = DateTime.Now,
                    Project = userProject.Project,
                    Text = questionToApi.Text,
                    AnswerToEmail = questionToApi.AnswerToEmail,
                    StatusUrl = questionToApi.StatusUrl
                };

                _applicationContext.Questions.Add(question);
                await _applicationContext.SaveChangesAsync();

                // Запускаем обработку вопроса без ожидания результата
                _processorService.FindAnswersForQuestionAsync(question.Id);

                var answer = new AnswerOnRequest
                {
                    QuestionId = question.Id,
                    StatusCode = Models.Api.StatusCode.Success,
                    StatusMessage = "Your question was successfully added. Wait for an answer to Status URL."
                };
                //answerJson = JsonConvert.SerializeObject(answer);
                return Json(answer);
            }
            catch (ErrorWithDataException ex)
            {
                var answer = new AnswerOnRequest()
                {
                    StatusCode = ex.StatusCode(),
                    StatusMessage = ex.Message
                };
                //var answerJson = JsonConvert.SerializeObject(answer);
                return Json(answer);
            }
            catch (Exception ex)
            {
                var answer = new AnswerOnRequest()
                {
                    StatusCode = Models.Api.StatusCode.Error,
                    StatusMessage = ex.Message
                };
                //var answerJson = JsonConvert.SerializeObject(answer);
                return Json(answer);
            }
        }
        /*
        [HttpPost]
        public async Task<string> Post([FromBody] QuestionPostModel questionPostModel)
        {
        }
        */

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
        //
        //// POST api/<controller>
        //[HttpPost]
        //public void Post([FromBody]string value)
        //{
        //}
        //
        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }


        /*
        
        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        
        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        */
    }
}

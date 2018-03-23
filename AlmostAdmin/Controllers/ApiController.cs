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
    [Route("api/[controller]")]
    public class ApiController : Controller
    {
        // TODO: возможность присылать ответ на вопрос указывая ИД вопроса
        // TODO: возможность запросить ответ на вопрос по айдишнику вопроса
        // TODO: возможность получить айдишник вопроса по полному тексту вопроса 1 в 1

        private MainService _mainService;
        private ApplicationContext _applicationContext;
        private ProcessorService _processorService;

        public ApiController(MainService mainService, ApplicationContext applicationContext, ProcessorService processorService)
        {
            _mainService = mainService;
            _applicationContext = applicationContext;
            _processorService = processorService;
        }

        public async Task<string> PostQuestion(string data, string signature)
        {
            try
            {
                string answerJson;
                var answer = new AnswerOnRequest();
                var decodedData = CryptoUtils.Base64Decode(data);
                var questionToApi = JsonConvert.DeserializeObject<QuestionToApi>(decodedData);
                
                if (questionToApi.IsModelValid())
                {
                    answer.StatusCode = Models.Api.StatusCode.WrongData;
                    answer.StatusMessage = "Some of the data parameters are invalid. Check the documentation.";
                    answerJson = JsonConvert.SerializeObject(answer);
                    return answerJson;
                }
                
                if(!Utils.ValidUrl(questionToApi.StatusUrl))
                {
                    answer.StatusCode = Models.Api.StatusCode.WrongStatusUrl;
                    answer.StatusMessage = "Provided URL is not valid.";
                    answerJson = JsonConvert.SerializeObject(answer);
                    return answerJson;
                }

                var user = _applicationContext.User
                    .Include(u => u.UserProjects)
                        .ThenInclude(up => up.Project)
                    .FirstOrDefault(u => u.UserName == questionToApi.Login);

                if(user == null || user.PasswordHash != questionToApi.PasswordHash)
                {
                    answer.StatusCode = Models.Api.StatusCode.WrongLoginPasswordCredentials;
                    answer.StatusMessage = "User with such email-password combination doesn't exist.";
                    answerJson = JsonConvert.SerializeObject(answer);
                    return answerJson;
                }
                
                var userProject = user.UserProjects.FirstOrDefault(up => up.Project.Id == questionToApi.ProjectId);

                if(userProject == null)
                {
                    answer.StatusCode = Models.Api.StatusCode.WrongProjectId;
                    answer.StatusMessage = "Provided user doesn't consist in the project with such ID.";
                    answerJson = JsonConvert.SerializeObject(answer);
                    return answerJson;
                }

                if (!Utils.ValidateSignature(data, signature, userProject.Project.PrivateKey))
                {
                    answer.StatusCode = Models.Api.StatusCode.WrongSignature;
                    answer.StatusMessage = "Signature is not valid. Check your PrivateKey and MD5 alghorithm.";
                    answerJson = JsonConvert.SerializeObject(answer);
                    return answerJson;
                }

                var question = new Question
                {
                    Date = DateTime.Now,
                    Project = userProject.Project,
                    Text = questionToApi.Text,
                    StatusUrl = questionToApi.StatusUrl
                };

                _applicationContext.Questions.Add(question);
                await _applicationContext.SaveChangesAsync();

                // Запускаем обработку вопроса без ожидания результата
                _processorService.ProcessQuestionAsync(question.Id);

                answer.QuestionId = question.Id;
                answer.StatusCode = Models.Api.StatusCode.Success;
                answer.StatusMessage = "Your question was successfully added. Wait for an answer to Status URL.";
                answerJson = JsonConvert.SerializeObject(answer);
                return answerJson;
            }
            catch(Exception ex)
            {
                var answer = new AnswerOnRequest()
                {
                    StatusCode = Models.Api.StatusCode.Error,
                    StatusMessage = ex.Message
                };
                var answerJson = JsonConvert.SerializeObject(answer);
                return answerJson;
            }
        }


        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

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

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

        /// <summary>
        /// Создать в системе вопрос и оставить заявку на получение ответа
        /// </summary>
        /// <param name="data"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> Post([FromForm] string data, [FromForm]string signature)
        {
            try
            {
                //string answerJson;
                var decodedData = CryptoUtils.Base64Decode(data);
                var questionToApi = JsonConvert.DeserializeObject<PostQuestion>(decodedData);

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
                await _processorService.FindAnswersForQuestionAsync(question.Id);

                var postQuestionResponse = new PostQuestionResponse
                {
                    QuestionId = question.Id,
                    StatusCode = Models.Api.StatusCode.Success,
                    StatusMessage = "Your question was successfully added. Wait for an answer to Status URL."
                };
                //answerJson = JsonConvert.SerializeObject(answer);
                return Json(postQuestionResponse);
            }
            catch (ErrorWithDataException ex)
            {
                var answer = new PostQuestionResponse()
                {
                    StatusCode = ex.StatusCode(),
                    StatusMessage = ex.Message
                };
                //var answerJson = JsonConvert.SerializeObject(answer);
                return Json(answer);
            }
            catch (Exception ex)
            {
                var answer = new PostQuestionResponse()
                {
                    StatusCode = Models.Api.StatusCode.Error,
                    StatusMessage = ex.Message
                };
                //var answerJson = JsonConvert.SerializeObject(answer);
                return Json(answer);
            }
        }

        // GET api/<controller>/5
        /// <summary>
        /// Получить вопрос и ответ на него по айдишнику
        /// </summary>
        /// <param name="data"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public JsonResult Get(string data, string signature)
        {
            try
            {
                var decodedData = CryptoUtils.Base64Decode(data);
                var getQuestionModel = JsonConvert.DeserializeObject<GetQuestion>(decodedData);

                if (!getQuestionModel.IsModelValid())
                {
                    throw new ErrorWithDataException("Some of the data parameters are invalid. Check the documentation.",
                        Models.Api.StatusCode.WrongData);
                }

                var user = _applicationContext.User
                    .Include(u => u.UserProjects)
                        .ThenInclude(up => up.Project)
                    .FirstOrDefault(u => u.UserName == getQuestionModel.Login);

                if (user == null)// || user.PasswordHash != questionToApi.PasswordHash)
                {
                    throw new ErrorWithDataException("User with such email doesn't exist.",
                        Models.Api.StatusCode.WrongLoginPasswordCredentials);
                }

                var userProject = user.UserProjects.FirstOrDefault(up => up.Project.Id == getQuestionModel.ProjectId);

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

                var question = _applicationContext.Questions
                    .Include(q => q.Answer)
                    .FirstOrDefault(q => q.Id == getQuestionModel.QuestionId);

                if(question == null)
                {
                    throw new ErrorWithDataException("Provided question ID doesn't consist in the project with such ID.",
                        Models.Api.StatusCode.WrongQuestionId);
                }

                var getQuestionResponse = new GetQuestionResponse
                {
                    QuestionId = question.Id,
                    Date = question.Date,
                    QuestionText = question.Text,
                    HasAnswer = question.Answer != null,
                    StatusCode = Models.Api.StatusCode.Success,
                    StatusMessage = "Such question exists.",
                    AnswerText = question.Answer != null ? question.Answer.Text : string.Empty
                };

                return Json(getQuestionResponse);
            }
            catch (ErrorWithDataException ex)
            {
                var answer = new PostQuestionResponse()
                {
                    StatusCode = ex.StatusCode(),
                    StatusMessage = ex.Message
                };

                return Json(answer);
            }
            catch (Exception ex)
            {
                var answer = new PostQuestionResponse()
                {
                    StatusCode = Models.Api.StatusCode.Error,
                    StatusMessage = ex.Message
                };

                return Json(answer);
            }
        }
        
        /*
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
        
        
        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        */
    }
}

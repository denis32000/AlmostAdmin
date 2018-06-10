using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlmostAdmin.Common;
using AlmostAdmin.Models;
using AlmostAdmin.Models.Api;
using AlmostAdmin.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AlmostAdmin.Controllers.Api
{
    [Route("api/[controller]")]
    public class AnswerController : Controller
    {
        //// GET: api/Answer
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        private ApplicationContext _applicationContext;
        private ProcessorService _processorService;

        public AnswerController(RepositoryService mainService, ApplicationContext applicationContext, ProcessorService processorService)
        {
            //_mainService = mainService;
            _applicationContext = applicationContext;
            _processorService = processorService;
        }

        /// <summary>
        /// Создать в системе ОТВЕТ определенному вопросу по ИД
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
                var answerToApi = JsonConvert.DeserializeObject<PostAnswer>(decodedData);

                if (!answerToApi.IsModelValid())
                {
                    throw new ErrorWithDataException("Some of the data parameters are invalid. Check the documentation.",
                        Models.Api.StatusCode.WrongData);
                }
                
                var user = _applicationContext.User
                    .Include(u => u.UserProjects)
                        .ThenInclude(up => up.Project)
                    .FirstOrDefault(u => u.UserName == answerToApi.Login);

                if (user == null)// || user.PasswordHash != questionToApi.PasswordHash)
                {
                    throw new ErrorWithDataException("User with such email doesn't exist.",
                        Models.Api.StatusCode.WrongLoginPasswordCredentials);
                }

                var userProject = user.UserProjects.FirstOrDefault(up => up.Project.Id == answerToApi.ProjectId);

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
                    .FirstOrDefault(q => q.Id == answerToApi.QuestionId);

                if (question == null)
                {
                    throw new ErrorWithDataException("Provided question ID doesn't consist in the project with such ID.",
                        Models.Api.StatusCode.WrongQuestionId);
                }

                var answer = new Answer
                {
                    Date = DateTime.Now,
                    Text = answerToApi.AnswerText,
                    ProjectId = answerToApi.ProjectId,
                    User = user
                };

                question.Answer = answer;
                question.AnsweredByHuman = true;

                _applicationContext.Answers.Add(answer);
                await _applicationContext.SaveChangesAsync();

                // Запускаем обработку вопроса без ожидания результата
                _processorService.AnswerOnSimilarQuestionsAsync(question.Id);

                var postAnswerResponse = new PostAnswerResponse
                {
                    QuestionId = question.Id,
                    StatusCode = Models.Api.StatusCode.Success,
                    StatusMessage = $"Answer on question with ID {question.Id} was successfully placed."
                };

                return Json(postAnswerResponse);
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
    }
}

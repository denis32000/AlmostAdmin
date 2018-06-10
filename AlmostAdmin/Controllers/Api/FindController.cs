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
    public class FindController : Controller
    {
        // TODO: возможность присылать ответ на вопрос указывая ИД вопроса
        // TODO: возможность запросить ответ на вопрос по айдишнику вопроса
        // TODO: возможность получить айдишник вопроса по полному тексту вопроса 1 в 1

        //private MainService _mainService;
        private ApplicationContext _applicationContext;
        private ProcessorService _processorService;

        public FindController(RepositoryService mainService, ApplicationContext applicationContext, ProcessorService processorService)
        {
            //_mainService = mainService;
            _applicationContext = applicationContext;
            _processorService = processorService;
        }
        
        // GET: api/<controller>
        /// <summary>
        /// Get list of similar questions to provided text
        /// </summary>
        /// <param name="data"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult Get(string data, /*[FromBody]string signature, */[FromQuery]string signature/*, [FromForm]string signature*/)
        {
            try
            {
                var decodedData = CryptoUtils.Base64Decode(data);
                var getQuestionsModel = JsonConvert.DeserializeObject<GetQuestions>(decodedData);

                if (!getQuestionsModel.IsModelValid())
                {
                    throw new ErrorWithDataException("Some of the data parameters are invalid. Check the documentation.",
                        Models.Api.StatusCode.WrongData);
                }

                var user = _applicationContext.User
                    .Include(u => u.UserProjects)
                        .ThenInclude(up => up.Project)
                    .FirstOrDefault(u => u.UserName == getQuestionsModel.Login);

                if (user == null)// || user.PasswordHash != questionToApi.PasswordHash)
                {
                    throw new ErrorWithDataException("User with such email doesn't exist.",
                        Models.Api.StatusCode.WrongLoginPasswordCredentials);
                }
                
                var userProject = user.UserProjects.FirstOrDefault(up => up.Project.Id == getQuestionsModel.ProjectId);

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

                var questionIds = _processorService.GetListOfSimilarQuestionIds(getQuestionsModel.Text, getQuestionsModel.ProjectId, getQuestionsModel.SimilarMaxCount);

                var questionList = _applicationContext.Questions
                    .Where(q => questionIds.Contains(q.Id))
                    .Select(q => q.Text)
                    .ToList();
                
                var getQuestionResponse = new GetQuestionsResponse
                {
                    QuestionText = getQuestionsModel.Text,
                    Questions = questionList,
                    StatusCode = Models.Api.StatusCode.Success,
                    StatusMessage = "List of similar questions."
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
        
    }
}

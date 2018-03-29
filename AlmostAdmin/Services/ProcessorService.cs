using AlmostAdmin.Common;
using AlmostAdmin.Data;
using AlmostAdmin.Models;
using AlmostAdmin.Models.Api;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Services
{
    public class ProcessorService
    {
        // TODO: используя ResponseSenderService мы отправляем ответы на вопросы на StatusUrl 
        // по мере поступления ответов от администраторов проекта

        private IntelligenceRequestAdapter _intelligenceRequestAdapter;
        private ApplicationContext _applicationContext;

        public ProcessorService(IntelligenceRequestAdapter intelligenceRequestAdapter, ApplicationContext applicationContext, ResponseSenderService responseSenderService)
        {
            _intelligenceRequestAdapter = intelligenceRequestAdapter;
            _applicationContext = applicationContext;
        }

        internal async Task<bool> ProcessQuestionAsync(int questionId)
        {
            // в АПИ прислали новый вопрос который нужно обработать по возможности
            //var question = _applicationContext.Questions.First(q => q.Id == questionId);
            //
            //var someIntelligenceValue = await _intelligenceRequestAdapter.ProcessDataAsync(question.Text);
            //
            //if(someIntelligenceValue.Success)
            //{
            //    question.InteligenceValue = someIntelligenceValue.Result;
            //
            //    // если хотя бы у одного из них есть ответ, то сразу возвращаем ответ клиенту
            //    var similarQuestionWithAnswer = _applicationContext.Questions
            //        .Include(q => q.Answer)
            //        .FirstOrDefault(q => q.InteligenceValue == question.InteligenceValue && q.Answer != null);
            //
            //    // TODO: впринципе это готовый ответ (similarQuestionWithAnswer)
            //    // TODO: сервис, который будет отправлять ПОСТ ответы на statusUrl
            //}

            //throw new NotImplementedException();
            return false;
        }

        internal async Task UpdateStatusForAllQuestions()
        {
            // получаем с базы все вопросы без ответа и пытаемся ответить на них
            throw new NotImplementedException();
        }

        internal async Task UpdateStatusForQuestion(int questionId)
        {
            // значит мы получили ответ в КОНТРОЛ_ПАНЕЛ на вопрос с этим идом, и можем отправить ОТВЕТ на статусУрл
            var question = _applicationContext.Questions
                .Include(q => q.Answer)
                .Include(q => q.Project)
                .FirstOrDefault(q => q.Id == questionId);

            if(question.AnswerToEmail)
            {
                var emailClient = new EmailService();
                emailClient.SendEmailAsync(
                    question.StatusUrl, 
                    $"Ответ на Ваш вопрос на {question.Project.Name}", 
                    question.Answer.Text, 
                    question.Project.Name);
                
                return;
            }

            var answerOnStatus = new AnswerOnStatusUrl
            {
                QuestionId = question.Id,
                AnswerText = question.Answer.Text,
                QuestionText = question.Text,
                StatusCode = question.AnsweredByHuman ? StatusCode.AnswerByHuman : StatusCode.AnswerBySystem
            };

            var answerOnStatusJson = JsonConvert.SerializeObject(answerOnStatus);
            var signature = CreateSignature(answerOnStatusJson, question.Project.PrivateKey);
            var data = CryptoUtils.Base64Encode(answerOnStatusJson);

            var request = new RestRequest(Method.POST);
            request.AddParameter("data", data);
            request.AddParameter("signature", signature);

            var response = new RestClient(question.StatusUrl).Execute(request);

            var result = response.Content;
        }

        private string CreateSignature(string jsonData, string privateKey)
        {
            var base64EncodedData = CryptoUtils.Base64Encode(jsonData);
            var stringToBeHashed = privateKey + base64EncodedData + privateKey;
            var sha1HashedString = CryptoUtils.HashSHA1(stringToBeHashed);
            var base64EncodedSha1String = CryptoUtils.Base64Encode(sha1HashedString);

            return base64EncodedSha1String;
        }

        internal async Task UpdateAnswerForQuestion(int questionId)
        {
            // значит мы получили ответ на вопрос ИЗ АПИ КОНТРОЛЛЕРА с этим идом, и можем сохранить его в базу
            throw new NotImplementedException();
        }
    }
}

using AlmostAdmin.Common;
using AlmostAdmin.Data;
using AlmostAdmin.Models;
using AlmostAdmin.Models.Api;
using Lucene.Net.Index;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Services
{
    public class ProcessorService
    {
        // TODO: используя ResponseSenderService мы отправляем ответы на вопросы на StatusUrl 
        // по мере поступления ответов от администраторов проекта

        private const float MinimalLuceneSimiliarity = 0.5f;// TODO: установить нижнюю границу похожести вопросов

        private LuceneProcessor _intelligenceRequestAdapter;
        private ApplicationContext _applicationContext;

        public ProcessorService(LuceneProcessor intelligenceRequestAdapter, 
            ApplicationContext applicationContext, 
            ResponseSenderService responseSenderService)
        {
            _intelligenceRequestAdapter = intelligenceRequestAdapter;
            _applicationContext = applicationContext;
        }

        /// <summary>
        /// Нам пришел вопрос и мы хотим его обработать
        /// запускаем алгоритм поиска похожих
        /// 2.1. если нашел похожий с ответом - присваиваем вопросу готовый ответ, НО не ставим статус "Отвечен человеком"
        /// 2.2 если не нашел - ждем ответа от модератора
        /// 
        /// Еще раз: я ищу похожие вопросы, отбираю на которые есть ответ от ЧЕЛОВЕКА, 
        /// и присваиваю ответ на вопрос от лица системы
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal async Task<bool> FindAnswersForQuestionAsync(int questionId, string path = "")
        {
            var question = _applicationContext.Questions.First(q => q.Id == questionId);

            if (_intelligenceRequestAdapter.GetDocumentsCount() == 0)
                _intelligenceRequestAdapter.BuildIndexWithExistingData(_applicationContext.Questions.ToList(), true);

            var ids = GetListOfSimilarQuestionIds(question.Text, question.ProjectId);

            var similarQuestions = _applicationContext.Questions
                .Include(q => q.Answer)
                .Where(q => ids.Contains(q.Id) && q.AnsweredByHuman)
                .ToList();
            
            if (similarQuestions.Count > 0)
            {
                question.Answer = similarQuestions.First().Answer;
                await _applicationContext.SaveChangesAsync();

                TrySendQuestionAnswerAsync(questionId);
            }

            _intelligenceRequestAdapter.AddDataToIndex(question);
            return false;
        }

        /// <summary>
        /// Когда ЧЕЛОВЕК дал ответ на какой то вопрос, я нашел все похожие, 
        /// и у которых совпадение больше минимума, я даю на них ответ от лица системы
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
        internal async Task AnswerOnSimilarQuestionsAsync(int questionId)
        {
            var question = _applicationContext.Questions
                .Include(q => q.Answer)
                .First(q => q.Id == questionId);

            var luceneSearchResults = _intelligenceRequestAdapter.Search(question.Text, question.ProjectId, 10);

            var ids = GetListOfSimilarQuestionIds(question.Text, question.ProjectId);

            var similarQuestions = _applicationContext.Questions
                .Include(q => q.Answer)
                .Where(q => ids.Contains(q.Id) && q.Answer == null) // проверяем, что бы они были неотвеченными
                .ToList();

            foreach(var similarQuestion in similarQuestions)
            {
                similarQuestion.Answer = question.Answer;
                await _applicationContext.SaveChangesAsync(); // TODO: rework it! bad perfomance
                TrySendQuestionAnswerAsync(similarQuestion.Id);
            }
        }
        
        internal IEnumerable<int> GetListOfSimilarQuestionIds(string questionText, int projectId, int resultsCount = 10)
        {
            var luceneSearchResults = _intelligenceRequestAdapter.Search(questionText, projectId, resultsCount);

            var ids = luceneSearchResults
                .Where(r => r.Score > MinimalLuceneSimiliarity)
                .Select(p => p.QuestionDbId);

            return ids;
        }

        internal async Task UpdateStatusForAllQuestions()
        {
            // получаем с базы все вопросы без ответа и пытаемся ответить на них
            throw new NotImplementedException();
        }

        /// <summary>
        /// Цель: отправить ОТВЕТ на статусУрл
        /// 
        /// Варианты, когда вызывается этот метод:
        /// 1. Мы получили ответ в КОНТРОЛ_ПАНЕЛ (от модератора)
        /// 2. Ответ на вопрос с этим идом из АПИ
        /// 3. Система сама нашла подходящий ответ на вопрос и попыталась отправить ответ, если проект это допускает
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
        internal async Task<bool> TrySendQuestionAnswerAsync(int questionId)
        {
            var question = _applicationContext.Questions
                .Include(q => q.Answer)
                .Include(q => q.Project)
                .FirstOrDefault(q => q.Id == questionId);

            if (question.Answer == null)
                return false;

            // Если проект запрещает отправлять ответ без подтверждения модератора И ответ дан не модератором
            if (!question.Project.AnswerWithoutApprove && !question.AnsweredByHuman)
                return false;

            if(question.AnswerToEmail)
            {
                var emailClient = new EmailService();
                await emailClient.SendEmailAsync(
                    question.StatusUrl, 
                    $"Ответ на вопрос. Проект {question.Project.Name}", 
                    question.Answer.Text, 
                    question.Project.Name);
                
                return true;
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

            var restClient = new RestClient(question.StatusUrl);
            var response = await Task.Run(() => restClient.Execute(request));

            //var result = response.Content;
            return response.IsSuccessful;
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

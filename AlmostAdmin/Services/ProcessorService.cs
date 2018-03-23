using AlmostAdmin.Data;
using AlmostAdmin.Models;
using Microsoft.EntityFrameworkCore;
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
            var question = _applicationContext.Questions.First(q => q.Id == questionId);

            var someIntelligenceValue = await _intelligenceRequestAdapter.ProcessDataAsync(question.Text);

            if(someIntelligenceValue.Success)
            {
                question.InteligenceValue = someIntelligenceValue.Result;

                // если хотя бы у одного из них есть ответ, то сразу возвращаем ответ клиенту
                var similarQuestionWithAnswer = _applicationContext.Questions
                    .Include(q => q.Answer)
                    .FirstOrDefault(q => q.InteligenceValue == question.InteligenceValue && q.Answer != null);

                // TODO: впринципе это готовый ответ (similarQuestionWithAnswer)
                // TODO: сервис, который будет отправлять ПОСТ ответы на statusUrl
            }

            throw new NotImplementedException();
        }

        internal async Task UpdateStatusForAllQuestions()
        {
            // получаем с базы все вопросы без ответа и пытаемся ответить на них
            throw new NotImplementedException();
        }

        internal async Task UpdateStatusForQuestion(int questionId)
        {
            // значит мы получили ответ в КОНТРОЛ_ПАНЕЛ на вопрос с этим идом, и можем отправить ОТВЕТ на статусУрл
            throw new NotImplementedException();
        }

        internal async Task UpdateAnswerForQuestion(int questionId)
        {
            // значит мы получили ответ на вопрос ИЗ АПИ КОНТРОЛЛЕРА с этим идом, и можем сохранить его в базу
            throw new NotImplementedException();
        }
    }
}

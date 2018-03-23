using AlmostAdmin.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Services
{
    public class ProcessorService
    {
        private IntelligenceRequestAdapter _intelligenceRequestAdapter;

        public ProcessorService(IntelligenceRequestAdapter intelligenceRequestAdapter)
        {
            _intelligenceRequestAdapter = intelligenceRequestAdapter;
        }

        internal async Task<bool> ProcessQuestionAsync(int questionId)
        {
            // в АПИ прислали новый вопрос который нужно обработать по возможности
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

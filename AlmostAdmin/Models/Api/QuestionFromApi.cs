using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Models.Api
{
    //public enum OperationType
    //{
    //    QuestionToApi,
    //    AnswerToApi
    //}

    public enum StatusCode
    {
        Success,
        Error,

        WrongLoginPasswordCredentials,
        WrongSignature,
        WrongData,
        WrongProjectId,
        WrongStatusUrl,

        // get response
        WrongQuestionId,

        AnswerByHuman,
        AnswerBySystem
    }

    public interface IApiRequest
    {
        bool IsModelValid();
        string Login { get; set; }
    }

    public interface IApiResponse
    {
        StatusCode StatusCode { get; set; }
        string StatusMessage { get; set; }
    }

    public class PostQuestion : IApiRequest
    {
        public int ProjectId { get; set; }
        public string Text { get; set; }
        public string StatusUrl { get; set; }
        public bool AnswerToEmail { get; set; }

        // IApiRequest
        public string Login { get; set; }
        public bool IsModelValid()
        {
            if (//Id > 0 && Id < Int32.MaxValue && 
                ProjectId > 0 && ProjectId < Int32.MaxValue && 
                !string.IsNullOrEmpty(Login) && 
                !string.IsNullOrEmpty(Text) && 
                !string.IsNullOrEmpty(StatusUrl))
                return true;

            return false;
        }
    }

    public class GetQuestion : IApiRequest
    {
        public int ProjectId { get; set; }
        public int QuestionId { get; set; }

        // IApiRequest
        public string Login { get; set; }
        public bool IsModelValid()
        {
            if (//Id > 0 && Id < Int32.MaxValue && 
                ProjectId > 0 && ProjectId < Int32.MaxValue &&
                QuestionId > 0 && QuestionId < Int32.MaxValue &&
                !string.IsNullOrEmpty(Login))
                return true;

            return false;
        }
    }

    public class GetQuestions : IApiRequest
    {
        public int ProjectId { get; set; }
        public string Text { get; set; }
        public int SimilarMaxCount { get; set; }

        // IApiRequest
        public string Login { get; set; }
        public bool IsModelValid()
        {
            if (//Id > 0 && Id < Int32.MaxValue && 
                ProjectId > 0 && ProjectId < Int32.MaxValue &&
                !string.IsNullOrEmpty(Login) &&
                !string.IsNullOrEmpty(Text) &&
                SimilarMaxCount > 0 && SimilarMaxCount < Int32.MaxValue)
                return true;

            return false;
        }
    }

    public class PostQuestionResponse : IApiResponse
    {
        public int QuestionId { get; set; }

        // IApiResponse
        public StatusCode StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }

    public class GetQuestionResponse : IApiResponse
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public DateTime Date { get; set; }
        public bool HasAnswer { get; set; }
        public string AnswerText { get; set; }

        // IApiResponse
        public StatusCode StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }

    public class GetQuestionsResponse : IApiResponse
    {
        public string QuestionText { get; set; }
        public List<string> Questions{ get; set; }

        // IApiResponse
        public StatusCode StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }


    public class PostAnswer : IApiRequest
    {
        public int ProjectId { get; set; }
        public int QuestionId { get; set; }
        public string AnswerText { get; set; }

        // IApiRequest
        public string Login { get; set; }
        public bool IsModelValid()
        {
            if (//Id > 0 && Id < Int32.MaxValue && 
                ProjectId > 0 && ProjectId < Int32.MaxValue &&
                !string.IsNullOrEmpty(Login))
                return true;

            return false;
        }
    }
    public class PostAnswerResponse : IApiResponse
    {
        public int QuestionId { get; set; }

        // IApiResponse
        public StatusCode StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }
    // =================================================================================

    public class AnswerOnStatusUrl
    {
        public int QuestionId { get; set; }
        //public OperationType OperationType { get; set; } // QuestionToApi / AnswerToApi
        public StatusCode StatusCode { get; set; }
        public string StatusMessage { get; set; }

        public string QuestionText { get; set; }
        public string AnswerText { get; set; }

        //public bool AnswerToEmail { get; set; }
    }
}

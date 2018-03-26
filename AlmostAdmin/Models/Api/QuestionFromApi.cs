using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlmostAdmin.Models.Api
{
    public interface IApiRequestModel
    {
        bool IsModelValid();
        string Login { get; set; }
    }

    public class QuestionToApi : IApiRequestModel
    {
        //public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Login { get; set; }
        public string Text { get; set; }
        public string StatusUrl { get; set; }

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

    /*
    public class AnswerToApi : IApiRequestModel
    {
        public int QuestionId { get; set; }
        public int ProjectId { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string Text { get; set; }
        public string StatusUrl { get; set; }

        public bool IsModelValid()
        {
            if (QuestionId > 0 && QuestionId < Int32.MaxValue
                && ProjectId > 0 && ProjectId < Int32.MaxValue
                && !string.IsNullOrEmpty(Login)
                && !string.IsNullOrEmpty(Text)
                && !string.IsNullOrEmpty(StatusUrl))
                return true;

            return false;
        }
    }
    */

    public enum OperationType
    {
        QuestionToApi,
        AnswerToApi
    }

    public enum StatusCode
    {
        Success,
        Error,

        WrongLoginPasswordCredentials,
        WrongSignature,
        WrongData,
        WrongProjectId,
        WrongStatusUrl
    }

    public class AnswerOnRequest
    {
        public int QuestionId { get; set; }
        public StatusCode StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }

    public class AnswerOnStatusUrl
    {
        public int QuestionId { get; set; }
        //public OperationType OperationType { get; set; } // QuestionToApi / AnswerToApi
        public StatusCode StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }
}

using System;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AlmostAdmin.Controllers
{
    public class ErrorWithDataException : Exception
    {
        private Models.Api.StatusCode _statusCode = Models.Api.StatusCode.Error;

        public Models.Api.StatusCode StatusCode()
        {
            return _statusCode;
        }

        public ErrorWithDataException(string message, Models.Api.StatusCode statusCode) : base(message)
        {
            _statusCode = statusCode;
        }
    }
}

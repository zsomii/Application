#region

using System;

#endregion

namespace Application.Infrastructure.Error
{
    public abstract class ServiceException : Exception
    {
        protected string Fault;
        protected string UserMessage;
        protected string TechnicalMessage;

        public abstract int GetCode();

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(new ServiceExceptionInfo(Fault, UserMessage,
                TechnicalMessage));
        }

        private class ServiceExceptionInfo
        {
            private string _fault;
            private string _userMessage;
            private string _technicalMessage;

            public ServiceExceptionInfo(string fault, string userMessage, string technicalMessage)
            {
                _fault = fault;
                _userMessage = userMessage;
                _technicalMessage = technicalMessage;
            }
        }
    }

    public class BadRequestException : ServiceException
    {
        private readonly int _code = 400;

        public BadRequestException(string userMessage = "SERVER_EXCEPTION_BAD_REQUEST", string technicalMessage = "")
        {
            Fault = "BAD_REQUEST";
            UserMessage = userMessage;
            TechnicalMessage = technicalMessage;
        }

        public override int GetCode()
        {
            return _code;
        }
    }

    public class UnauthorizedException : ServiceException
    {
        private readonly int _code = 401;

        public UnauthorizedException(string userMessage = "SERVER_EXCEPTION_UNAUTHORIZED", string technicalMessage = "",
            string fault = "UNAUTHORIZED")
        {
            Fault = fault;
            UserMessage = userMessage;
            TechnicalMessage = technicalMessage;
        }

        public override int GetCode()
        {
            return _code;
        }
    }

    public class InvalidLoginException : ServiceException
    {
        private readonly int _code = 412;

        public InvalidLoginException(string userMessage = "SERVER_EXCEPTION_INVALID_LOGIN", string technicalMessage = "")
        {
            Fault = "UNAUTHORIZED";
            UserMessage = userMessage;
            TechnicalMessage = technicalMessage;
        }

        public override int GetCode()
        {
            return _code;
        }
    }

    public class PasswordExpiredException : ServiceException
    {
        private readonly int _code = 512;

        public PasswordExpiredException(string userMessage = "SERVER_EXCEPTION_PASSWORD_EXPIRED", string technicalMessage = "")
        {
            Fault = "PASSWORD_EXPIRED";
            UserMessage = userMessage;
            TechnicalMessage = technicalMessage;
        }

        public override int GetCode()
        {
            return _code;
        }
    }

    public class LoginTimeoutException : ServiceException
    {
        private readonly int _code = 440;

        public LoginTimeoutException(string userMessage = "SERVER_EXCEPTION_LOGIN_TIMEOUT", string technicalMessage = "")
        {
            Fault = "LOGIN_TIMEOUT";
            UserMessage = userMessage;
            TechnicalMessage = technicalMessage;
        }

        public override int GetCode()
        {
            return _code;
        }
    }

    public class TokenExpiredException : ServiceException
    {
        private readonly int _code = 492;

        public TokenExpiredException(string userMessage = "SERVER_EXCEPTION_TOKEN_EXPIRED", string technicalMessage = "")
        {
            Fault = "TOKEN_EXPIRED";
            UserMessage = userMessage;
            TechnicalMessage = technicalMessage;
        }

        public override int GetCode()
        {
            return _code;
        }
    }

    public class ForbiddenException : ServiceException
    {
        private readonly int _code = 403;

        public ForbiddenException(string userMessage = "SERVER_EXCEPTION_FORBIDDEN", string technicalMessage = "")
        {
            Fault = "FORBIDDEN";
            UserMessage = userMessage;
            TechnicalMessage = technicalMessage;
        }

        public override int GetCode()
        {
            return _code;
        }
    }

    public class NotFoundException : ServiceException
    {
        private readonly int _code = 404;

        public NotFoundException(string userMessage = "SERVER_EXCEPTION_NOT_FOUND", string technicalMessage = "")
        {
            Fault = "NOT_FOUND";
            UserMessage = userMessage;
           TechnicalMessage = technicalMessage;
        }

        public override int GetCode()
        {
            return _code;
        }
    }

    public class GeneralException : ServiceException
    {
        private readonly int _code = 500;

        public GeneralException(string userMessage = "SERVER_EXCEPTION_SERVER_ERROR", string technicalMessage = "")
        {
            Fault = "SERVER_ERROR";
            UserMessage = userMessage;
            TechnicalMessage = technicalMessage;
        }

        public override int GetCode()
        {
            return _code;
        }
    }
}
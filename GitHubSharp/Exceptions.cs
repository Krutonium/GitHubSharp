using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;

namespace GitHubSharp
{
    public class ForbiddenException : StatusCodeException
    {
        public ForbiddenException(string message, Dictionary<string, string> headers = null)
        : base(HttpStatusCode.Forbidden, message, headers) { }
    }

    public class NotFoundException : StatusCodeException
    {
        public NotFoundException(string message, Dictionary<string, string> headers = null)
        : base(HttpStatusCode.NotFound, message, headers) { }
    }

    public class NotModifiedException : StatusCodeException
    {
        public NotModifiedException(string message, Dictionary<string, string> headers = null)
        : base(HttpStatusCode.NotModified, message, headers) { }
    }

    public class UnauthorizedException : StatusCodeException
    {
        public UnauthorizedException(string message, Dictionary<string, string> headers = null)
        : base(HttpStatusCode.Unauthorized, message, headers) { }
    }

    public class InternalServerException : StatusCodeException
    {
        public InternalServerException(string message, Dictionary<string, string> headers = null)
        : base(HttpStatusCode.InternalServerError, message, headers) { }
    }

    public class StatusCodeException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public Dictionary<string, string> Headers { get; private set; }

        public StatusCodeException(HttpStatusCode statusCode, Dictionary<string, string> headers)
            : this(statusCode, statusCode.ToString(), headers)
        {
        }

        public StatusCodeException(HttpStatusCode statusCode, string message, Dictionary<string, string> headers)
            : base(message)
        {
            StatusCode = statusCode;
            Headers = headers;
        }

        internal static StatusCodeException FactoryCreate(RestSharp.IRestResponse response)
        {
            var headers = new Dictionary<string, string>();
            foreach (var h in response.Headers)
                headers.Add(h.Name, h.Value.ToString());

            string errorStr = null;
            try
            {
                errorStr = new RestSharp.Deserializers.JsonDeserializer().Deserialize<GitHubSharp.Models.ErrorModel>(response).Message;
            }
            catch 
            {
                //Do nothing
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.Forbidden:
                    return new ForbiddenException(errorStr ?? "You do not have the permissions to access or modify this resource.", headers);
                case HttpStatusCode.NotFound:
                    return new NotFoundException(errorStr ?? "The server is unable to locate the requested resource.", headers);
                case HttpStatusCode.InternalServerError:
                    return new InternalServerException(errorStr ?? "The request was unable to be processed due to an interal server error.", headers);
                case HttpStatusCode.Unauthorized:
                    return new UnauthorizedException(errorStr ?? "You are unauthorized to view the requested resource.", headers);
                case HttpStatusCode.NotModified:
                    return new NotModifiedException(errorStr ?? "This resource has not been modified since the last request.", headers);
                default:
                    return new StatusCodeException(response.StatusCode, errorStr ?? response.StatusCode.ToString(), headers);
            }
        }
    }
}


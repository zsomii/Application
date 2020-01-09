#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Application.Domain.Service;
using Application.Infrastructure.Error;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

#endregion

namespace Application.Infrastructure.Middleware
{
    public class HttpHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly object SyncObject = new object();
        private const string RequestIdHeader = "x-request-id";
        private const string ContentDisposition = "Content-Disposition";
        private const string AccessControlExposeHeaders = "Access-Control-Expose-Headers";
        private const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";

        private readonly Lazy<ILogService> _logService;

        public HttpHandlerMiddleware(RequestDelegate next, Lazy<ILogService> logService)
        {
            _next = next;
            _logService = logService;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                var requestId = context.Request.Headers.ContainsKey(RequestIdHeader)
                    ? context.Request.Headers[RequestIdHeader]
                    : new StringValues(string.Empty);
                context.Response.Headers.Add(RequestIdHeader, requestId);
                KeyValuePair<string, StringValues> corsHeader =
                    context.Response.Headers.SingleOrDefault(a => a.Key.Equals(AccessControlAllowOrigin));
                if (!string.IsNullOrWhiteSpace(corsHeader.Key))
                {
                    context.Response.Headers.Remove(corsHeader);
                }

                context.Response.Headers.Add(AccessControlAllowOrigin, "*");
                context.Response.Headers.Add(AccessControlExposeHeaders,
                    new StringValues(new List<string> { RequestIdHeader, ContentDisposition }.ToArray()));
                return Task.FromResult(0);
            });
            try
            {
                await _next(context);
                switch (context.Response.StatusCode)
                {
                    case 401:
                        throw new UnauthorizedException();
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            lock (SyncObject)
            {
                var injectedRequestStream = new MemoryStream();
                string result;
                try
                {
                    var code = (int) HttpStatusCode.InternalServerError;
                    if (exception is ServiceException serviceException)
                    {
                        code = serviceException.GetCode();
                    }

                    result = exception.ToString();
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = code;

                    using (var bodyReader = new StreamReader(context.Request.Body))
                    {
                        var requestLog = bodyReader.ReadToEnd();
                        byte[] bytesToWrite = Encoding.UTF8.GetBytes(requestLog);
                        injectedRequestStream.Write(bytesToWrite, 0, bytesToWrite.Length);
                        injectedRequestStream.Seek(0, SeekOrigin.Begin);
                        context.Request.Body = injectedRequestStream;
                    }
                }
                finally
                {
                    _logService.Value.CreateExceptionLog(GetRequestData(context), exception.StackTrace);
                    injectedRequestStream.Dispose();
                }

                return context.Response.WriteAsync(result);
            }
        }

        private static string GetRequestData(HttpContext context)
        {
            var sb = new StringBuilder();
            if (context.Request.HasFormContentType && context.Request.Form.Any())
            {
                sb.Append("Form variables:");
                foreach (var (key, value) in context.Request.Form)
                {
                    sb.AppendFormat("Key={0}, Value={1}<br/>", key, value);
                }
            }

            sb.AppendLine("Method: " + context.Request.Method);
            return sb.ToString();
        }
    }
}
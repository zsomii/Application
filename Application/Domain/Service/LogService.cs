#region

using System;
using System.Collections.Generic;
using System.Linq;
using Application.Data.Entity.Enumerations;
using Application.Data.Entity.Log;
using Application.Data.Repository;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

#endregion

namespace Application.Domain.Service
{
    public interface ILogService
    {
        // Create
        void CreateSingleLog(Log log);
        void CreateExceptionLog(string request, string stackTrace);

        // List
        List<Log> GetLogs(LogLevel? logLevel = null, string keyword = null, int page = 0);
        List<Log> GetLatestLogs();
    }

    public class LogService : ILogService
    {
        private readonly IBaseRepository<Log> _logRepository;
        private const int PageSize = 50;

        public LogService(IServiceProvider services)
        {
            var scope = services.CreateScope();
            _logRepository = scope.ServiceProvider.GetRequiredService<IBaseRepository<Log>>();
        }

        public void CreateSingleLog(Log log)
        {
            _logRepository.CreateOrUpdateEntity(log);
        }

        public void CreateExceptionLog(string request, string stackTrace)
        {
            var log = new Log
            {
                TimeStamp = DateTime.UtcNow,
                Message = request + " " + stackTrace ,
                LogLevel = LogLevel.ERROR,
                LogType = LogType.COMMUNICATION,
            };

            _logRepository.CreateOrUpdateEntity(log);
        }

        public List<Log> GetLogs(LogLevel? logLevel = null, string keyword = null, int page = 0)
        {
            throw new NotImplementedException();
        }


        
        public void CreateAuthenticationLog(LogLevel logLevel, string message)
        {
            var log = new Log(DateTime.UtcNow, logLevel: logLevel, message: message, logType: LogType.AUTHENTICATION);

            _logRepository.CreateOrUpdateEntity(log);
        }

        public List<Log> GetLogs(LogLevel? logLevel = null,
            LogType? logType = null, string machineId = null, string username = null, string keyword = null,
            int page = 0)
        {
            return _logRepository.GetList()
                .Where(l => logLevel == null || l.LogLevel == logLevel)
                .Where(l => logType == null || l.LogType == logType)
                .Where(l => username == null || l.UserId == username)
                .Where(l => keyword == null || OtherFieldsContainKeyword(l, keyword)).Skip(page * PageSize)
                .Take(PageSize).OrderByDescending(x => x.TimeStamp).ToList();
        }
        
        public List<Log> GetLatestLogs()
        {
            return _logRepository.GetList().OrderByDescending(x => x.TimeStamp).Take(100).ToList();
        }

        #region Private functions

        private static bool OtherFieldsContainKeyword(Log log, string keyword)
        {
            return LogPropertyContainsKeyword(log.Message, keyword);
        }

        private static bool LogPropertyContainsKeyword(string propertyValue, string keyword)
        {
            return propertyValue?.Contains(keyword) ?? false;
        }

        #endregion
    }
}
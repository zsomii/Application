#region

using System;
using System.Diagnostics;
using Application.Data;
using Application.Data.Dto.Authorization;
using Application.Data.Entity.Authorization;
using Application.Data.Entity.Log;
using Application.Data.Entity.Todo;
using Application.Data.Repository;
using Application.Domain.Service;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace Application.Infrastructure
{
    public static class Installer
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>();

            // BaseRepository
            services.AddTransient<IBaseRepository<Todo>, TodoRepository>();
            services.AddTransient<IBaseRepository<Log>, LogRepository>();

            // PrivilegeRepository
            services.AddTransient<IPrivilegeRepository<Permission>, PermissionRepository>();

            // Services
            services.AddTransient<ILogService, LogService>();

            // Lazy services
            services.AddTransient(provider => new Lazy<ILogService>(provider.GetService<ILogService>));

            // PrivilegeService
            services.AddTransient<IPrivilegeService<Permission, PermissionDto>, PermissionService>();
        }
    }
}
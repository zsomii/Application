#region

using System.Security.Claims;
using Application.Data.Entity.User;
using Application.Domain.Service;
using Application.Infrastructure.Error;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace Application.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IMapper Mapper;
        protected readonly IAccountService AccountService;
        protected readonly ILogService LogService;

        public BaseController(IMapper mapper, IAccountService accountService, ILogService logService)
        {
            Mapper = mapper;
            AccountService = accountService;
            LogService = logService;
        }

        protected User CheckUser(HttpRequest request, ClaimsPrincipal user, string permission = null)
        {
            var u = AccountService.ValidateUser(Request, User);
            if (!string.IsNullOrWhiteSpace(permission) && !AccountService.HasPermission(u.UserName, permission))
            {
                throw new ForbiddenException();
            }

            return u;
        }
    }
}
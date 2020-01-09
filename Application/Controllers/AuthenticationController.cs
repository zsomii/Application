#region

using System;
using System.Threading.Tasks;
using Application.Data.Dto.Authentication;
using Application.Data.Entity.Enumerations;
using Application.Data.Entity.User;
using Application.Domain.Service;
using Application.Infrastructure.Error;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

#endregion

namespace Application.Controllers
{
    [Produces("application/json")]
    [Route("auth")]
    public class AuthenticationController : BaseController
    {
        private readonly SignInManager<User> _signInManager;

        public AuthenticationController(IAccountService accountService,
            SignInManager<User> signInManager,
            IMapper mapper,
            ILogService logService
        ) : base(mapper, accountService, logService)
        {
            _signInManager = signInManager;
        }

        [HttpPost("login")]
        public LoginResponseDto Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.UserName) ||
                string.IsNullOrWhiteSpace(loginDto.Password))
            {
                LogService.CreateAuthenticationLog(LogLevel.ERROR, JsonConvert.SerializeObject(loginDto));
                throw new BadRequestException();
            }

            return AccountService.Login(loginDto);
        }

        [HttpGet("is-logged-in")]
        public void UserIsLoggedIn()
        {
            AccountService.ValidateUser(Request, User);
        }

        [HttpPost("logout")]
        public async Task Logout()
        {
            try
            {
                AccountService.ValidateUser(Request, User);
            }
            catch (Exception)
            {
                // ignored
            }

            await _signInManager.SignOutAsync();
        }
    }
}
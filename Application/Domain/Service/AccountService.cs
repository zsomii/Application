using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Data.Dto.Authentication;
using Application.Data.Dto.User;
using Application.Data.Entity.Enumerations;
using Application.Data.Entity.User;
using Application.Data.Repository;
using Application.Data.Token;
using Application.Domain.Util;
using Application.Infrastructure.Error;
using Application.Infrastructure.Token;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.Domain.Service
{
    public interface IAccountService
    {
        List<UserDto> ListUsers();
        UserDto FindUserByName(string userName);
        UserDto CreateUser(string currentHostName, UserDto user, string languageCode);
        UserDto ModifyUser(UserDto user);
        User FindUser(ClaimsPrincipal user);
        User FindUser(Claim user);
        User FindUser(string userId);
        User ValidateUser(HttpRequest request, ClaimsPrincipal user);
        User FindUserByEmail(string email);
        LoginResponseDto Login(LoginDto loginDto, string currentHost, string languageId);
        bool HasPermission(string userName, string permission);
        void InvalidateUserWithPermission(string permissionId);
        void InvalidateUserWithRole(string name);
    }

    public class AccountService : BaseService, IAccountService
    {
        private const int InactivityTimeInMinutes = 6;

        private readonly IRoleService _roleService;
        private readonly ITokenService _tokenService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountService(
            ITokenService tokenService,
            IRoleService roleService,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IMapper mapper) : base(mapper)

        {
            _roleService = roleService;
            _tokenService = tokenService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public List<UserDto> ListUsers()
        {
            return _userManager.Users.ToList().Select(user =>
            {
                var userDto = Mapper.Map<UserDto>(user);
                userDto.Roles = _roleService.FindRolesByUser(user);
                return userDto;
            }).ToList();
        }

        public UserDto FindUserByName(string userName)
        {
            var user = _userManager.Users.SingleOrDefault(a => a.UserName.Equals(userName));
            if (user == null)
            {
                throw new GeneralException("USER_MESSAGE_USER_NOT_FOUND", userName);
            }
            var userToAdd = Mapper.Map<User, UserDto>(user);
            userToAdd.Roles = _roleService.FindRolesByUser(user);
            return userToAdd;
        }

        public User FindUserByEmail(string email)
        {
            var user = _userManager.Users.SingleOrDefault(a => a.Email.Equals(email));
            if (user == null)
            {
                throw new GeneralException("USER_MESSAGE_USER_WITH_EMAIL_NOT_EXISTS", email);
            }
            return user;
        }

        public bool UsernameAlreadyExists(string username)
        {
            return _userManager.Users.Any(x => x.UserName == username);
        }

        public bool EmailAlreadyExists(string email)
        {
            return _userManager.Users.Any(x => x.Email == email);
        }

        public UserDto CreateUser(string currentHostName, UserDto userDto, string languageCode)
        {
            if (UsernameAlreadyExists(userDto.UserName))
            {
                throw new GeneralException("USER_MESSAGE_USER_NAME_ALREADY_EXISTS", userDto.UserName);
            }

            if (EmailAlreadyExists(userDto.Email))
            {
                throw new GeneralException("USER_MESSAGE_EMAIL_ALREADY_USE", userDto.Email);
            }

            foreach (var role in userDto.Roles)
            {
                _roleService.FindRoleByName(role.Name);
            }
            var user = Mapper.Map<UserDto, User>(userDto);
            user.Status = UserStatus.EXPIRED;
            var createResult = _userManager.CreateAsync(user).Result;
            if (!createResult.Succeeded)
            {
                throw new GeneralException(createResult.GetErrors(), "TECHNICAL_MESSAGE_CREATING_USER");
            }

            var rolesDto = _roleService.ModifyUserRoles(user, userDto.Roles.Select(a => a.Name).ToList());
            var retVal = Mapper.Map<User, UserDto>(user);
            retVal.Roles = rolesDto;
            return retVal;
        }

        public UserDto ModifyUser(UserDto userDto)
        {
            var findResult = FindUserByName(userDto.UserName);
            if (findResult == null)
            {
                throw new GeneralException("USER_MESSAGE_USER_NAME_ALREADY_EXISTS", userDto.UserName);
            }
            if (userDto.Email != null)
            {
                ValidateUserEmail(userDto.Email);
            }
            var user = _userManager.Users.Single(a => a.UserName.Equals(userDto.UserName));

            user.Nickname = userDto.Nickname;

            if (userDto.Email != null)
            {
                user.Email = userDto.Email;
            }

            _tokenService.InvalidateToken(user, TokenInvalidationReason.USER_MODIFIED);
            if (user.Status == UserStatus.LOCKED && userDto.Status != UserStatusDto.LOCKED)
            {
                user.LockoutEnd = null;
            }
            user.Status = Mapper.Map<UserStatusDto, UserStatus>(userDto.Status);
            var updateResult = _userManager.UpdateAsync(user).Result;
            if (!updateResult.Succeeded)
            {
                throw new GeneralException(updateResult.GetErrors(), "TECHNICAL_MESSAGE_UPDATING_USER");
            }

            var modifiedUser = FindUserByName(user.UserName);
            modifiedUser.Roles = _roleService.ModifyUserRoles(user, userDto.Roles.Select(a => a.Name).ToList());
            return modifiedUser;
        }

        private void ValidateUserNameAndEmail(string userName, string email)
        {
            var findResult = FindUserByName(userName);
            if (findResult != null)
            {
                throw new GeneralException("USER_MESSAGE_USER_NAME_ALREADY_EXISTST", userName);
            }
            if (_userManager.Users.Any(a => a.Email.Equals(email)))
            {
                throw new GeneralException("USER_MESSAGE_EMAIL_ALREADY_USE", email);
            }
        }

        private void ValidateUserEmail(string email)
        {
            if (_userManager.Users.Any(a => a.Email.Equals(email)))
            {
                throw new GeneralException("USER_MESSAGE_EMAIL_ALREADY_USE", email);
            }
        }

        public User FindUser(ClaimsPrincipal User)
        {
            Claim userId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return null;
            }

            return GetUser(userId);
        }

        public User FindUser(string userId)
        {
            if (userId == null)
            {
                return null;
            }

            return _userManager.Users.SingleOrDefault(a => a.Id.Equals(userId));
        }

        private User GetUser(Claim userId)
        {
            return _userManager.Users.SingleOrDefault(a => a.Id.Equals(userId.Value));
        }

        public User FindUser(Claim claim)
        {
            return GetUser(claim);
        }

        public User ValidateUser(HttpRequest request, ClaimsPrincipal user)
        {
            CheckUser(user);
            var u = FindUser(user);
            CheckUserStatus(u);
            CheckUserInactivityTime(u);
            u.LastRequestTime = DateTime.Now;
            var _ = _userManager.UpdateAsync(u).Result;
            return u;
        }

        private static Claim CheckUser(ClaimsPrincipal User)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                var tokenInvalidationReason = User.FindFirst(L3JwtSecurityTokenHandler.TOKEN_INVALIDATION_REASON);
                var reason = tokenInvalidationReason == null ? null : "TECHNICAL_MESSAGE_TOKEN_INVALIDATED_" + tokenInvalidationReason.Value;
                if (reason != null)
                {
                    throw new UnauthorizedException("USER_MESSAGE_AUTO_LOGOUT", reason);
                }

                throw new UnauthorizedException();
            }

            return userId;
        }


        private static void CheckUserStatus(User user)
        {
            if (user.Status != UserStatus.ACTIVE)
            {
                throw new InvalidLoginException("USER_MESSAGE_ACCOUNT_STATUS_ERROR", user.Status.ToString());
            }
        }

        public LoginResponseDto Login(LoginDto loginDto, string currentHost, string languageId)
        {
            var appUser = _userManager.FindByNameAsync(loginDto.UserName).Result;
            if (appUser == null)
            {
                throw new InvalidLoginException("USER_MESSAGE_USER_NOT_FOUND", loginDto.UserName);
            }

            if (string.IsNullOrWhiteSpace(appUser.PasswordHash))
            {
                throw new InvalidLoginException(technicalMessage: "TECHNICAL_MESSAGE_ACCOUNT_NOT_CONFIRMED");
            }

            if (appUser.Status != UserStatus.ACTIVE)
            {
                throw new InvalidLoginException("USER_MESSAGE_ACCOUNT_STATUS_ERROR", appUser.Status.ToString());
            }
            var result = _signInManager.PasswordSignInAsync(loginDto.UserName, loginDto.Password, false, true).Result;
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    appUser.Status = UserStatus.LOCKED;
                    var _ = _userManager.UpdateAsync(appUser).Result;
                    throw new InvalidLoginException(technicalMessage: "TECHNICAL_MESSAGE_ACCOUNT_LOCKED_OUT");
                }
                if (result.IsNotAllowed)
                {
                    throw new InvalidLoginException(technicalMessage: "TECHNICAL_MESSAGE_ACCOUNT_LOGIN_NOT_ALLOWED");
                }
                if (result.RequiresTwoFactor)
                {
                    throw new InvalidLoginException(technicalMessage: "TECHNICAL_MESSAGE_TWO_FACTOR_AUTH_REQUIRED");
                }

                throw new InvalidLoginException();
            }

            var userDto = Mapper.Map<User, UserDto>(appUser);
            userDto.Roles = _roleService.FindRolesByUser(appUser);

            var token = _tokenService.GenerateJwtToken(loginDto.UserName, appUser);
            appUser.LastRequestTime = DateTime.Now;
            var r = _userManager.UpdateAsync(appUser).Result;
            return new LoginResponseDto() { Token = token, User = userDto };
        }
        
        public bool HasPermission(string userName, string permission)
        {
            var appUser = _userManager.FindByNameAsync(userName).Result;
            if (appUser == null)
            {
                throw new InvalidLoginException("USER_MESSAGE_USER_NOT_FOUND", userName);
            }
            var roles = _roleService.FindRolesByUser(appUser);
            foreach (var roleDto in roles)
            {
                if (roleDto.Permissions.FirstOrDefault(item => item.Name == permission) != null)
                {
                    return true;
                }
            }

            return false;
        }
        
        public void InvalidateUserWithPermission(string permissionId)
        {
            var users = ListUsers();

            foreach (var user in users)
            {
                var invalidated = false;
                foreach (var role in user.Roles)
                {
                    if (role.Permissions.Any(x => x.Id == permissionId))
                    {
                        _tokenService.InvalidateToken(Mapper.Map<UserDto, User>(user), TokenInvalidationReason.PERMISSION_MODIFIED);
                        invalidated = true;
                    }

                    if (invalidated)
                    {
                        break;
                    }
                }
            }
        }

        public void InvalidateUserWithRole(string name)
        {
            var users = ListUsers();

            foreach (var user in users)
            {
                if (user.Roles.Any(x => x.Name == name))
                {
                    _tokenService.InvalidateToken(Mapper.Map<UserDto, User>(user), TokenInvalidationReason.ROLE_MODIFIED);
                }
            }
        }

        private void CheckUserInactivityTime(User user)
        {
            if ((DateTime.Now - user.LastRequestTime).TotalMinutes > InactivityTimeInMinutes)
            {
                throw new LoginTimeoutException();
            }
        }
    }
}

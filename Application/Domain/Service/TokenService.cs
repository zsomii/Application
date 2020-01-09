#region

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Data.Entity.User;
using Application.Data.Token;
using Application.Infrastructure.Error;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

#endregion

namespace Application.Domain.Service
{
    public interface ITokenService
    {
        Guid InvalidateToken(User user, TokenInvalidationReason reason);
        string GenerateJwtToken(string userName, User user);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;

        public TokenService(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public Guid InvalidateToken(User user, TokenInvalidationReason reason)
        {
            var actualUser = _userManager.FindByEmailAsync(user.Email).Result;
            actualUser.ValidTokenStamp = Guid.NewGuid();
            actualUser.SecurityStamp = Guid.NewGuid().ToString();

            if (reason != TokenInvalidationReason.NONE)
            {
                actualUser.TokenInvalidationReason = reason.ToString();
            }

            var result = _userManager.UpdateAsync(actualUser).Result;
            if (!result.Succeeded)
            {
                throw new GeneralException(result.Errors.ToString(), "TECHNICAL_MESSAGE_UPDATING_USER");
            }

            return user.ValidTokenStamp;
        }

        public string GenerateJwtToken(string userName, User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtAudience"],
                claims,
                expires: expires,
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
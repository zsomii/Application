#region

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Application.Domain.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

#endregion

namespace Application.Infrastructure.Token
{
    public class L3JwtSecurityTokenHandler : ISecurityTokenValidator
    {
        public static string VALID_TOKEN_STAMP = "ValidTokenStamp";
        public static string ORGANIZATION_NAME = "OrganizationId";
        public static string TOKEN_INVALIDATION_REASON = "TokenInvalidationReason";

        private static L3JwtSecurityTokenHandler instance;

        public static L3JwtSecurityTokenHandler INSTANCE
        {
            get
            {
                if (instance == null)
                {
                    instance = new L3JwtSecurityTokenHandler();
                }

                return instance;
            }
        }

        public IServiceProvider ServiceProvider { get; set; }


        private readonly JwtSecurityTokenHandler _tokenHandler;

        private L3JwtSecurityTokenHandler()
        {
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public bool CanValidateToken => true;

        public int MaximumTokenSizeInBytes { get; set; } = TokenValidationParameters.DefaultMaximumTokenSizeInBytes;

        public bool CanReadToken(string securityToken)
        {
            return _tokenHandler.CanReadToken(securityToken);
        }

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters,
            out SecurityToken validatedToken)
        {
            var token = _tokenHandler.ReadJwtToken(securityToken);
            var validTokenStamp = token.Claims.FirstOrDefault(a => a.Type.Equals(VALID_TOKEN_STAMP));
            var organization = token.Claims.FirstOrDefault(a => a.Type.Equals(ORGANIZATION_NAME));
            var userIdClaim = token.Claims.FirstOrDefault(a => a.Type.Equals(ClaimTypes.NameIdentifier));

            using (var scope = ServiceProvider.CreateScope())
            {
                var accountService = scope.ServiceProvider.GetService<IAccountService>();
                ClaimsPrincipal claimsPrincipal;

                try
                {
                    claimsPrincipal =
                        _tokenHandler.ValidateToken(securityToken, validationParameters, out validatedToken);
                }
                catch (Exception)
                {
                    validatedToken = null;
                    return new ClaimsPrincipal();
                }

                if (userIdClaim == null || validTokenStamp == null)
                {
                    return new ClaimsPrincipal();
                }
             
                var user = accountService.FindUser(userIdClaim);

                if (user == null || !validTokenStamp.Value.Equals(user.ValidTokenStamp.ToString()))
                {
                    claimsPrincipal = new ClaimsPrincipal();
                }

                if (user != null && claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier) == null &&
                    !string.IsNullOrWhiteSpace(user.TokenInvalidationReason))
                {
                    List<Claim> claims = new List<Claim>
                    {
                        new Claim(TOKEN_INVALIDATION_REASON, user.TokenInvalidationReason)
                    };

                    var identity = new ClaimsIdentity(claims);

                    claimsPrincipal = new ClaimsPrincipal(identity);
                }

                return claimsPrincipal;
            }
        }
    }
}
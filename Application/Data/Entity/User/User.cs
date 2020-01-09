#region

using System;
using Application.Data.Entity.Enumerations;
using Microsoft.AspNetCore.Identity;

#endregion

namespace Application.Data.Entity.User
{
    public class User : IdentityUser
    {
        public string Nickname { get; set; }
        public UserStatus Status { get; set; }
        public DateTime LastRequestTime { get; set; }

        public Guid LastVerificationStamp { get; set; }

        public Guid ValidTokenStamp { get; set; }

        public string TokenInvalidationReason { get; set; }

        public DateTime LastPasswordChangedTime { get; set; }
    }
}
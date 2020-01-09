#region

using System.Collections.Generic;
using Application.Data.Dto.Authorization;

#endregion

namespace Application.Data.Dto.User
{
    public class UserDto : BaseDto
    {
        public string Nickname { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public UserStatusDto Status { get; set; }
        public List<RoleDto> Roles { get; set; }
    }
}
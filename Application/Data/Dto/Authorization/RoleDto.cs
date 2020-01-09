#region

using System.Collections.Generic;

#endregion

namespace Application.Data.Dto.Authorization
{
    public class RoleDto : BaseDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<PermissionDto> Permissions { get; set; }
    }
}
#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace Application.Data.Entity.Authorization
{
    [Table("Permission")]
    public class Permission : Privilege
    {
        public PermissionType PermissionType { get; set; }

        public List<RolePrivilege<Permission>> RolePermissions { get; set; }
    }
}
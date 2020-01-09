#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

#endregion

namespace Application.Data.Entity.Authorization
{
    public class Role : IdentityRole
    {
        public string Description { get; set; }

        public List<RolePrivilege<Permission>> RolePermissions { get; set; }
        [NotMapped] public EntityOperation Operation { get; set; }
    }
}
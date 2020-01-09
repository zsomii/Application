using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Data.Entity.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Application.Data.Repository
{
    public class PermissionRepository : PrivilegeRepository<Permission>
    {
        public PermissionRepository(AppDbContext context) : base(context) { }

        protected override DbSet<Permission> GetEntityTable()
        {
            return Context.Permissions;
        }

        public override Permission CreateOrUpdateEntity(Permission entity)
        {
            var existingPermission = GetEntityTable().FirstOrDefault(x => x.Name == entity.Name);
            if (existingPermission != null)
            {
                entity.Id = existingPermission.Id;
                base.DetachEntity<Permission>(existingPermission);
            }

            return base.CreateOrUpdateEntity(entity);
        }

        public override List<Permission> CreateOrUpdateEntities(List<Permission> entities)
        {
            foreach (var entity in entities)
            {
                var existingPermission = GetEntityTable().FirstOrDefault(x => x.Name == entity.Name);
                if (existingPermission != null)
                {
                    entity.Id = existingPermission.Id;
                }
            }

            base.DetachEntities<Permission>(entities);
            return base.CreateOrUpdateEntities(entities);
        }

    }
}

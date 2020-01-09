using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Data.Dto.Authorization;
using Application.Data.Entity.Authorization;
using Application.Data.Repository;
using AutoMapper;

namespace Application.Domain.Service
{
    public class PermissionService : PrivilegeService<Permission, PermissionDto>
    {

        protected override void SetDynamicProperties(PermissionDto privilegeDto, Permission privilege)
        {
            privilege.PermissionType = _mapper.Map<PermissionType>(privilegeDto.PermissionType);
        }

        public PermissionService(IPrivilegeRepository<Permission> repository, IMapper mapper) :
            base(repository, mapper)
        {

        }
    }
}

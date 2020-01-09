using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Application.Data.Dto.Authorization;
using Application.Data.Entity.Authorization;
using Application.Data.Entity.Enumerations;
using Application.Data.Repository;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Domain.Service
{
    public interface IPrivilegeService<TPrivilege, TPrivilegeDto> where TPrivilege : Privilege, new() where TPrivilegeDto : PrivilegeDto
    {
        List<TPrivilege> ListPrivileges();
        TPrivilegeDto CreateOrUpdateEntity(TPrivilegeDto privilege, string languageCode);
        void DeletePrivilege(string id);
        TPrivilege GetPrivilegeById(string id);
        TPrivilege GetPrivilegeByName(string name);
        List<TPrivilegeDto> CreateOrUpdateEntities(List<TPrivilegeDto> privilegeDtos, string languageId);
    }

    public abstract class PrivilegeService<TPrivilege, TPrivilegeDto> : IPrivilegeService<TPrivilege, TPrivilegeDto> where TPrivilege : Privilege, new() where TPrivilegeDto : PrivilegeDto
    {
        protected readonly IPrivilegeRepository<TPrivilege> _repository;
        protected readonly IMapper _mapper;


        protected PrivilegeService(IPrivilegeRepository<TPrivilege> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public virtual TPrivilegeDto CreateOrUpdateEntity(TPrivilegeDto privilegeDto, string languageId)
        {
            if (privilegeDto == null)
            {
                // TODO specify
                throw new Exception();
            }

            var relatedSystem = RelatedSystem.NONE;
            if (privilegeDto != null && privilegeDto.GetType() == typeof(PermissionDto))
            {
                var permissionType = (privilegeDto as PermissionDto).PermissionType;
                if (permissionType.HasFlag(PermissionTypeDto.CLIENT))
                {
                    relatedSystem |= RelatedSystem.CLIENT;
                }
                if (permissionType.HasFlag(PermissionTypeDto.SERVER))
                {
                    relatedSystem |= RelatedSystem.SERVER;
                }
            }

            var privilege = new TPrivilege()
            {
                Name = privilegeDto.Name,
                Description = privilegeDto.Description,
                Id = privilegeDto.Id
            };
            SetDynamicProperties(privilegeDto, privilege);
            return _mapper.Map<TPrivilegeDto>(_repository.CreateOrUpdateEntity(privilege));
        }

        public virtual List<TPrivilegeDto> CreateOrUpdateEntities(List<TPrivilegeDto> privilegeDtos, string languageId)
        {
            if (privilegeDtos == null)
            {
                // TODO specify
                throw new Exception();
            }

            var privileges = new List<TPrivilege>();
            foreach (var privilegeDto in privilegeDtos)
            {
             
                var relatedSystem = RelatedSystem.NONE;
                if (privilegeDto != null && privilegeDto.GetType() == typeof(PermissionDto))
                {
                    var permissionType = (privilegeDto as PermissionDto).PermissionType;
                    if (permissionType.HasFlag(PermissionTypeDto.CLIENT))
                    {
                        relatedSystem |= RelatedSystem.CLIENT;
                    }
                    if (permissionType.HasFlag(PermissionTypeDto.SERVER))
                    {
                        relatedSystem |= RelatedSystem.SERVER;
                    }
                }

                var privilege = new TPrivilege()
                {
                    Name = privilegeDto.Name,
                    Description = privilegeDto.Description,
                    Id = privilegeDto.Id
                };
                SetDynamicProperties(privilegeDto, privilege);
                privileges.Add(privilege);
            }

            return _mapper.Map<List<TPrivilegeDto>>(_repository.CreateOrUpdateEntities(privileges));
        }



        protected abstract void SetDynamicProperties(TPrivilegeDto privilegeDto, TPrivilege privilege);

        public void DeletePrivilege(string id)
        {
            _repository.DeleteEntity(id);
        }

        public TPrivilege GetPrivilegeById(string id)
        {
            return _repository.GetList().FirstOrDefault(item => item.Id == id);
        }

        public TPrivilege GetPrivilegeByName(string name)
        {
            return _repository.GetList().FirstOrDefault(item => item.Name == name);
        }

        public List<TPrivilege> ListPrivileges()
        {
            return _repository.GetList().ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Application.Data.Dto.Authorization;
using Application.Data.Entity.Authorization;
using Application.Data.Entity.User;
using Application.Data.Token;
using Application.Domain.Util;
using Application.Infrastructure.Error;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Domain.Service
{
    public interface IRoleService
    {
        RoleDto CreateRole(string name, string description, string languageId);
        bool DeleteRole(string roleName);
        Role FindRoleByName(string name);
        RoleDto FindRoleDtoByName(string name);
        List<RoleDto> FindRolesByUser(User user);
        List<RoleDto> ListRoles();
        List<RoleDto> ModifyUserRoles(User user, List<string> roles);
    }

    public class RoleService : BaseService, IRoleService
    {
        private const string ROLE_DESCRIPTIONS = "ROLE_DESCRIPTIONS";
        private const string ROLE_LABELGROUP_NAME = "ROLES";

        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IPrivilegeService<Permission, PermissionDto> _permissionService;
        private readonly ITokenService _tokenService;

        public RoleService(
            RoleManager<Role> roleManager,
            UserManager<User> userManager,
            ITokenService tokenService,
            IPrivilegeService<Permission, PermissionDto> permissionService,
            IMapper mapper
            ) : base(mapper)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _tokenService = tokenService;
            _permissionService = permissionService;
        }
        public RoleDto CreateRole(string name, string description, string languageId)
        {
            var findResult = _roleManager.FindByNameAsync(name).Result;
            if (findResult != null)
            {
                throw new GeneralException("USER_MESSAGE_ROLE_ALREADY_EXISTS", name);
            }
            var roleToCreate = new Role() { Name = name, Description = description };
            var createResult = _roleManager.CreateAsync(roleToCreate).Result;
            if (!createResult.Succeeded)
            {
                throw new GeneralException(createResult.GetErrors(), "TECHNICAL_MESSAGE_CREATING_ROLE");
            }
            return ConvertRoleToRoleDto(roleToCreate);
        }
        
        public bool DeleteRole(string roleName)
        {
            var role = FindRoleByName(roleName);
            var deleteResult = _roleManager.DeleteAsync(role).Result;

            if (!deleteResult.Succeeded)
            {
                throw new GeneralException(deleteResult.GetErrors(), "TECHNICAL_MESSAGE_DELETING_ROLE");
            }
            return true;
        }

        public Role FindRoleByName(string roleName)
        {
            var findResult = _roleManager.Roles.FirstOrDefault(item => item.Name == roleName);

            if (findResult == null)
            {
                throw new GeneralException("USER_MESSAGE_ROLE_NOT_FOUND", roleName);
            }
            return findResult;
        }

        public RoleDto FindRoleDtoByName(string name)
        {
            return ConvertRoleToRoleDto(FindRoleByName(name));
        }

        public List<RoleDto> FindRolesByUser(User user)
        {
            var userRoles = _userManager.GetRolesAsync(user).Result;
            return userRoles.Select(FindRoleByName).Select(ConvertRoleToRoleDto).ToList();
        }

        public List<RoleDto> ListRoles()
        {
            
                return _roleManager.Roles.Include(item => item.Description)
                    .Include(item => item.RolePermissions)
                    .ThenInclude(item => item.Privilege)
                    .ThenInclude(item => item.Description)
                    .Select(role => ConvertRoleToRoleDto(role)).ToList();
         
        }
        

        private static void SetRolePrivileges<TPrivilege, TPrivilegeDto>(string roleId, IReadOnlyCollection<TPrivilegeDto> privileges, List<RolePrivilege<TPrivilege>> rolePrivileges) where TPrivilege : Privilege where TPrivilegeDto : PrivilegeDto
        {
            if (privileges != null)
            {
                foreach (var privilege in privileges)
                {
                    if (!rolePrivileges.Any(rp => rp.PrivilegeId.Equals(privilege.Id)))
                    {
                        rolePrivileges.Add(new RolePrivilege<TPrivilege>() { PrivilegeId = privilege.Id, RoleId = roleId });
                    }
                }
            }
            rolePrivileges.RemoveAll(item => privileges == null || !privileges.Any(p => p.Id.Equals(item.PrivilegeId)));
        }



        public List<RoleDto> ModifyUserRoles(User user, List<string> roles)
        {
            var userRoles = _userManager.GetRolesAsync(user).Result;

            var rolesToAdd = new List<string>();

            foreach (var role in roles)
            {
                if (!userRoles.Contains(role))
                {
                    rolesToAdd.Add(role);
                }
            }
            var rolesToRemove = userRoles.Where(role => !roles.Contains(role)).ToList();

            if (rolesToRemove.Count > 0)
            {
                var removeResult = _userManager.RemoveFromRolesAsync(user, rolesToRemove).Result;
                if (!removeResult.Succeeded)
                {
                    throw new GeneralException(removeResult.GetErrors(), "TECHNICAL_MESSAGE_REMOVING_USER_FROM_ROLE");
                }
            }

            if (rolesToAdd.Count > 0)
            {
                var addResult = _userManager.AddToRolesAsync(user, rolesToAdd).Result;
                if (!addResult.Succeeded)
                {
                    throw new GeneralException(addResult.GetErrors(), "TECHNICAL_MESSAGE_ADDING_USER_TO_ROLE");
                }
            }

            if (rolesToRemove.Count > 0)
            {
                var _ = _tokenService.InvalidateToken(user, TokenInvalidationReason.ROLE_MODIFIED);
            }
            return GetRolesByUser(user);
        }

        private List<RoleDto> GetRolesByUser(User user)
        {
            var userRoles = _userManager.GetRolesAsync(user).Result;
            return _roleManager.Roles.Include(item => item.Description)
                .Include(item => item.RolePermissions).Where(a => userRoles.Contains(a.Name)).Select(role => ConvertRoleToRoleDto(role)).ToList();
        }

        private RoleDto ConvertRoleToRoleDto(Role role)
        {

            List<PermissionDto> permissions = role.RolePermissions?.Select(p => new PermissionDto()
            {
                Id = p.PrivilegeId,
                Description = p.Privilege?.Description,
                PermissionType = Mapper.Map<PermissionTypeDto>(p.Privilege?.PermissionType),
                Name = p.Privilege?.Name
            }).ToList();

            return new RoleDto()
            {
                Description = role.Description,
                Name = role.Name,
                Permissions = permissions
            };
        }

    }
}

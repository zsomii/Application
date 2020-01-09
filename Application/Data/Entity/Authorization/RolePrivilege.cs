namespace Application.Data.Entity.Authorization
{
    public class RolePrivilege<T> : BaseEntity where T : Privilege
    {
        public string RoleId { get; set; }
        public Role Role { get; set; }

        public string PrivilegeId { get; set; }

        public T Privilege { get; set; }
    }
}
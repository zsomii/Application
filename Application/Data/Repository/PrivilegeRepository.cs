#region

using System.Linq;
using Application.Data.Entity.Authorization;

#endregion

namespace Application.Data.Repository
{
    public interface IPrivilegeRepository<T> : IBaseRepository<T> where T : Privilege
    {
        T GetPrivilegeByName(string name);
    }

    public abstract class PrivilegeRepository<T> : BaseRepository<T>, IPrivilegeRepository<T> where T : Privilege
    {
        public T GetPrivilegeByName(string name)
        {
            return GetEntityTable().SingleOrDefault(a => a.Name.Equals(name));
        }

        protected PrivilegeRepository(AppDbContext context) : base(context)
        {
        }
    }
}
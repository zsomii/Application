#region

using Application.Data.Entity.Todo;
using Microsoft.EntityFrameworkCore;

#endregion

namespace Application.Data.Repository
{
    public class TodoRepository : BaseRepository<Todo>
    {
        public TodoRepository(AppDbContext context) : base(context)
        {
        }

        protected override DbSet<Todo> GetEntityTable()
        {
            return Context.Todo;
        }
    }
}
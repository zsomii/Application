#region

using Application.Data.Entity.Log;
using Microsoft.EntityFrameworkCore;

#endregion

namespace Application.Data.Repository
{
    public class LogRepository : BaseRepository<Log>
    {
        public LogRepository(AppDbContext context) : base(context)
        {
        }

        protected override DbSet<Log> GetEntityTable()
        {
            return Context.Logs;
        }
    }
}
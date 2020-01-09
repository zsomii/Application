#region

using System.Linq;
using Microsoft.AspNetCore.Identity;

#endregion

namespace Application.Domain.Util
{
    public static class Extensions
    {
        #region Identity result

        public static string GetErrors(this IdentityResult result)
        {
            return string.Concat(result.Errors.AsEnumerable().Select(x => x.Description + " ").ToArray());
        }

        #endregion
    }
}
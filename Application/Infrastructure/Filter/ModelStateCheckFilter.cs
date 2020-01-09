#region

using System.Linq;
using Application.Infrastructure.Error;
using Microsoft.AspNetCore.Mvc.Filters;

#endregion

namespace Application.Infrastructure.Filter
{
    public sealed class ModelStateCheckFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                throw new BadRequestException(technicalMessage: string.Join("; ",
                    context.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.Exception.Message)));
            }
        }
    }
}
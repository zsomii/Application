#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace Application.Data.Entity.Authorization
{
    public class Privilege : BaseEntity
    {
        [Required] public string Name { get; set; }
        public string Description { get; set; }
    }
}
#region

using System;

#endregion

namespace Application.Data.Entity.Todo
{
    public class Todo : BaseEntity
    {
        // On 1-5 scale, 5 has the highest priority
        public int Priority { get; set; }
        public string Task { get; set; }
        public DateTime Deadline { get; set; }
    }
}
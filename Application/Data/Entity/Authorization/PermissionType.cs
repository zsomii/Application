#region

using System;

#endregion

namespace Application.Data.Entity.Authorization
{
    [Flags]
    public enum PermissionType
    {
        NONE = 0,
        CLIENT = 1 << 0,
        SERVER = 1 << 1
    }
}
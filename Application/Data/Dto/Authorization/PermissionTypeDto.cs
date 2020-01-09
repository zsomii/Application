#region

using System;

#endregion

namespace Application.Data.Dto.Authorization
{
    [Flags]
    public enum PermissionTypeDto
    {
        NONE = 0,
        CLIENT = 1 << 0,
        SERVER = 1 << 1
    }
}
namespace Application.Data.Token
{
    public enum TokenInvalidationReason
    {
        NONE,
        ROLE_MODIFIED,
        PERMISSION_MODIFIED,
        USER_MODIFIED
    }
}
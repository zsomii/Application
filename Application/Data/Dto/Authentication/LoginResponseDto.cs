#region

using Application.Data.Dto.User;

#endregion

namespace Application.Data.Dto.Authentication
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public UserDto User { get; set; }
    }
}
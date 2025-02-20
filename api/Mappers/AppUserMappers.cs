using api.Dtos.AppUser;
using api.Models;

namespace api.Mappers;

public static class AppUserMappers
{
    public static AppUser ToAppUserModel(this RegisterUserDto registerUserDto)
    {
        return new AppUser
        {
            UserName = registerUserDto.UserName,
            Email = registerUserDto.Email,
        };
    }
}
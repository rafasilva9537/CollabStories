using System.Linq.Expressions;
using api.Dtos.AppUser;
using api.Models;

namespace api.Mappers;

public static class AppUserMappers
{
    // Model to Dto
    public static AppUserDto ToAppUserDto(this AppUser appUser)
    {
        return new AppUserDto
        {
            UserName = appUser.UserName,
            Nickname = appUser.Nickname,
            CreatedDate = appUser.CreatedDate,
            Email = appUser.Email,
        };
    }

    public static UserMainInfoDto ToUserMainInfoDto(this AppUser appUser)
    {
        return new UserMainInfoDto
        {
            UserName = appUser.UserName,
            Nickname = appUser.Nickname,
            CreatedDate = appUser.CreatedDate,
        };
    }

    public static Expression<Func<AppUser, UserMainInfoDto>> ProjetToUserMainInfoDto = (appUser) => new UserMainInfoDto {
        UserName = appUser.UserName,
        Nickname = appUser.Nickname,
        CreatedDate = appUser.CreatedDate,
    };


    // Dto to Model
    public static AppUser ToAppUserModel(this RegisterUserDto registerUserDto)
    {
        return new AppUser
        {
            UserName = registerUserDto.UserName,
            Email = registerUserDto.Email,
        };
    }
}
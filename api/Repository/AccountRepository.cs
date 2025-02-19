using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using api.Models;
using api.Dtos.AppUser;

namespace api.Repository
{
    public interface IAccountRepository
    {
        Task<bool> RegisterAsync(RegisterUserDto registerUser);
    }

    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<AppUser> _userManager;
        public AccountRepository(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public Task<bool> RegisterAsync(RegisterUserDto registerUser)
        {
            // TODO: implement
            throw new NotImplementedException();
        }
    }
}
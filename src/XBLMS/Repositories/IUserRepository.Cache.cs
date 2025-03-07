﻿using System.Threading.Tasks;
using XBLMS.Dto;
using XBLMS.Models;

namespace XBLMS.Repositories
{
    public partial interface IUserRepository
    {
        Task<User> GetByAccountAsync(string account);

        Task<User> GetByUserIdAsync(int userId);

        Task<User> GetByUserNameAsync(string userName);

        Task<User> GetByMobileAsync(string mobile);

        Task<User> GetByEmailAsync(string email);

        Task<User> GetByGuidAsync(string guid);

        Task<string> GetDisplayAsync(int userId);

        string GetDisplay(User user);
        Task<(int allCount, int addCount, int deleteCount, int lockedCount, int unLockedCount)> GetDataCount(AuthorityAuth auth);
    }
}

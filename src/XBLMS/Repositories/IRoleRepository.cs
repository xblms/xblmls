using Datory;
using System.Collections.Generic;
using System.Threading.Tasks;
using XBLMS.Dto;
using XBLMS.Models;

namespace XBLMS.Repositories
{
    public interface IRoleRepository : IRepository
    {
        Task<Role> GetRoleAsync(int roleId);

        Task<List<Role>> GetRolesAsync(AuthorityAuth auth);


        Task<int> InsertRoleAsync(Role role);

        Task UpdateRoleAsync(Role role);

        Task<bool> DeleteRoleAsync(int roleId);

        Task<bool> IsRoleExistsAsync(string roleName);

        bool IsPredefinedRole(string roleName);
    }
}

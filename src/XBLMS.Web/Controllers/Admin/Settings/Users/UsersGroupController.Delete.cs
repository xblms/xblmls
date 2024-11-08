﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using XBLMS.Dto;
using XBLMS.Core.Utils;
using XBLMS.Enums;
using XBLMS.Utils;

namespace XBLMS.Web.Controllers.Admin.Settings.Users
{
    public partial class UsersGroupController
    {
        [HttpPost, Route(RouteDelete)]
        public async Task<ActionResult<BoolResult>> Delete([FromBody] IdRequest request)
        {
            if (!await _authManager.HasPermissionsAsync(MenuPermissionType.Delete))
            {
                return this.NoAuth();
            }
            var group = await _userGroupRepository.GetAsync(request.Id);

            await _userGroupRepository.DeleteAsync(group.Id);

            await _authManager.AddAdminLogAsync("删除用户组", $"{group.GroupName}");

            return new BoolResult
            {
                Value = true
            };
        }
    }
}

﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using XBLMS.Configuration;
using XBLMS.Dto;
using XBLMS.Enums;
using XBLMS.Utils;

namespace XBLMS.Web.Controllers.Admin.Settings.Utilities
{
    public partial class UtilitiesCacheController
    {
        [HttpPost, Route(RouteRestart)]
        public async Task<ActionResult<BoolResult>> Restart()
        {
            if (_settingsManager.IsSafeMode)
            {
                return this.Error(Constants.ErrorSafe);
            }

            if (!await _authManager.HasPermissionsAsync(MenuPermissionType.SystemRestart))
            {
                return this.NoAuth();
            }
            if (!await _authManager.HasPermissionsAsync())
            {
                return this.NoAuth();
            }

            await _authManager.AddAdminLogAsync("重启系统");
            await _authManager.AddStatLogAsync(StatType.None, "重启系统");

            _hostApplicationLifetime.StopApplication();



            return new BoolResult
            {
                Value = true
            };
        }
    }
}

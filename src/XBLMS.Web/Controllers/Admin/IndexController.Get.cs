﻿using Datory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using XBLMS.Configuration;
using XBLMS.Utils;

namespace XBLMS.Web.Controllers.Admin
{
    public partial class IndexController
    {
        [HttpGet, Route(Route)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] GetRequest request)
        {
            var allowed = PageUtils.IsVisitAllowed(_settingsManager, Request);
            if (!allowed)
            {
                return this.Error($"访问已被禁止，IP地址：{PageUtils.GetIpAddress(Request)}，请与管理员联系开通访问权限");
            }

            var (redirect, redirectUrl) = await AdminRedirectCheckAsync();
            if (redirect)
            {
                return new GetResult
                {
                    Value = false,
                    RedirectUrl = redirectUrl
                };
            }

            var admin = await _authManager.GetAdminAsync();
            if (admin == null)
            {
                return Unauthorized();
            }
            var cacheKey = Constants.GetSessionIdCacheKey(admin.Id);
            var sessionId = await _dbCacheRepository.GetValueAsync(cacheKey);
            if (string.IsNullOrEmpty(request.SessionId) || sessionId != request.SessionId)
            {
                return Unauthorized();
            }

            var allMenus = await _authManager.GetMenus();

            if (allMenus == null || allMenus.Count == 0) return this.Error($"无访问权限，请与管理员联系开通访问权限");

            var config = await _configRepository.GetAsync();

            var isEnforcePasswordChange = false;
            if (config.IsAdminEnforcePasswordChange)
            {
                if (admin.LastChangePasswordDate == null)
                {
                    isEnforcePasswordChange = true;
                }
                else
                {
                    var ts = new TimeSpan(DateTime.Now.Ticks - admin.LastChangePasswordDate.Value.Ticks);
                    if (ts.TotalDays > config.AdminEnforcePasswordChangeDays)
                    {
                        isEnforcePasswordChange = true;
                    }
                }
            }

            var curOrganName = "";
            var IsOrganAdmin = false;
            if (admin.Auth == Enums.AuthorityType.Admin || admin.Auth == Enums.AuthorityType.AdminCompany)
            {
                if (admin.AuthCurManageOrganId > 0)
                {
                    var organ = await _organCompanyRepository.GetAsync(admin.AuthCurManageOrganId);
                    if (organ != null)
                    {
                        curOrganName = organ.Name;
                    }
                }
                else
                {
                    admin.AuthCurManageOrganId = admin.AuthManageOrganId = admin.CompanyId;
                    await _administratorRepository.UpdateAsync(admin);

                    var organ = await _organCompanyRepository.GetAsync(admin.AuthCurManageOrganId);
                    if (organ != null)
                    {
                        curOrganName = organ.Name;
                    }
                }

                IsOrganAdmin = true;
            }
            else if (admin.Auth == Enums.AuthorityType.AdminDepartment)
            {
                if (admin.AuthCurManageOrganId > 0)
                {
                    var organ = await _organDepartmentRepository.GetAsync(admin.AuthCurManageOrganId);
                    if (organ != null)
                    {
                        curOrganName = organ.Name;
                    }

                }
                else
                {
                    admin.AuthCurManageOrganId = admin.AuthManageOrganId = admin.DepartmentId;
                    await _administratorRepository.UpdateAsync(admin);

                    var organ = await _organDepartmentRepository.GetAsync(admin.AuthCurManageOrganId);
                    if (organ != null)
                    {
                        curOrganName = organ.Name;
                    }
                }
                IsOrganAdmin = true;
            }

            return new GetResult
            {
                IsSafeMode = _settingsManager.IsSafeMode,
                Value = true,
                Menus = allMenus,
                IsEnforcePasswordChange = isEnforcePasswordChange,
                Local = new Local
                {
                    UserId = admin.Id,
                    Guid = admin.Guid,
                    UserName = admin.UserName,
                    DisplayName = admin.DisplayName,
                    AvatarUrl = admin.AvatarUrl,
                    Auth = admin.Auth.GetDisplayName(),
                    CurOrganName = curOrganName,
                    IsOrganAdmin = IsOrganAdmin
                },
                Version = _settingsManager.Version

            };
        }
    }
}

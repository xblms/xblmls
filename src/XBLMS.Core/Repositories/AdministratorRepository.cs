﻿using Datory;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using XBLMS.Core.Utils;
using XBLMS.Dto;
using XBLMS.Enums;
using XBLMS.Models;
using XBLMS.Repositories;
using XBLMS.Services;
using XBLMS.Utils;

namespace XBLMS.Core.Repositories
{
    public partial class AdministratorRepository : IAdministratorRepository
    {
        private readonly Repository<Administrator> _repository;
        private readonly IConfigRepository _configRepository;
        private readonly IAdministratorsInRolesRepository _administratorsInRolesRepository;
        private readonly IRoleRepository _roleRepository;

        public AdministratorRepository(ISettingsManager settingsManager, IConfigRepository configRepository,
            IAdministratorsInRolesRepository administratorsInRolesRepository, IRoleRepository roleRepository)
        {
            _repository = new Repository<Administrator>(settingsManager.Database, settingsManager.Redis);
            _configRepository = configRepository;
            _administratorsInRolesRepository = administratorsInRolesRepository;
            _roleRepository = roleRepository;
        }

        public IDatabase Database => _repository.Database;

        public string TableName => _repository.TableName;

        public List<TableColumn> TableColumns => _repository.TableColumns;
        public async Task<bool> ExistsAsync(int id)
        {
            return await _repository.ExistsAsync(id);
        }
        public async Task UpdateLastActivityDateAndCountOfFailedLoginAsync(Administrator administrator)
        {
            if (administrator == null) return;

            administrator.LastActivityDate = DateTime.Now;
            administrator.CountOfFailedLogin += 1;

            var cacheKeys = GetCacheKeys(administrator);

            await _repository.UpdateAsync(Q
                .Set(nameof(Administrator.LastActivityDate), administrator.LastActivityDate)
                .Set(nameof(Administrator.CountOfFailedLogin), administrator.CountOfFailedLogin)
                .Where(nameof(Administrator.Id), administrator.Id)
                .CachingRemove(cacheKeys.ToArray())
            );
        }

        public async Task UpdateLastActivityDateAsync(Administrator administrator)
        {
            if (administrator == null) return;

            administrator.LastActivityDate = DateTime.Now;

            var cacheKeys = GetCacheKeys(administrator);

            await _repository.UpdateAsync(Q
                .Set(nameof(Administrator.LastActivityDate), administrator.LastActivityDate)
                .Where(nameof(Administrator.Id), administrator.Id)
                .CachingRemove(cacheKeys.ToArray())
            );
        }

        public async Task UpdateLastActivityDateAndCountOfLoginAsync(Administrator administrator)
        {
            if (administrator == null) return;

            administrator.LastActivityDate = DateTime.Now;
            administrator.CountOfLogin += 1;
            administrator.CountOfFailedLogin = 0;

            var cacheKeys = GetCacheKeys(administrator);

            await _repository.UpdateAsync(Q
                .Set(nameof(Administrator.LastActivityDate), administrator.LastActivityDate)
                .Set(nameof(Administrator.CountOfLogin), administrator.CountOfLogin)
                .Set(nameof(Administrator.CountOfFailedLogin), administrator.CountOfFailedLogin)
                .Where(nameof(Administrator.Id), administrator.Id)
                .CachingRemove(cacheKeys.ToArray())
            );
        }

        private async Task ChangePasswordAsync(Administrator administrator, PasswordFormat passwordFormat, string passwordSalt, string password)
        {
            administrator.LastChangePasswordDate = DateTime.Now;

            var cacheKeys = GetCacheKeys(administrator);

            await _repository.UpdateAsync(Q
                .Set(nameof(Administrator.Password), password)
                .Set(nameof(Administrator.PasswordFormat), passwordFormat.GetValue())
                .Set(nameof(Administrator.PasswordSalt), passwordSalt)
                .Set(nameof(Administrator.LastChangePasswordDate), administrator.LastChangePasswordDate)
                .Where(nameof(Administrator.Id), administrator.Id)
                .CachingRemove(cacheKeys.ToArray())
            );
        }

        public async Task LockAsync(IList<string> userNames)
        {
            var cacheKeys = new List<string>();
            foreach (var userName in userNames)
            {
                var administrator = await GetByUserNameAsync(userName);
                cacheKeys.AddRange(GetCacheKeys(administrator));
            }

            await _repository.UpdateAsync(Q
                .Set(nameof(Administrator.Locked), true)
                .WhereIn(nameof(Administrator.UserName), userNames)
                .CachingRemove(cacheKeys.ToArray())
            );
        }

        public async Task UnLockAsync(IList<string> userNames)
        {
            var cacheKeys = new List<string>();
            foreach (var userName in userNames)
            {
                var administrator = await GetByUserNameAsync(userName);
                cacheKeys.AddRange(GetCacheKeys(administrator));
            }

            await _repository.UpdateAsync(Q
                .Set(nameof(Administrator.Locked), false)
                .Set(nameof(Administrator.CountOfFailedLogin), 0)
                .WhereIn(nameof(Administrator.UserName), userNames)
                .CachingRemove(cacheKeys.ToArray())
            );
        }
        public async Task<int> GetCountAsync(int companyId, int departmentId, int dutyId)
        {
            var query = Q.NewQuery();
            if (companyId > 0)
            {
                query.Where(nameof(Administrator.CompanyId), companyId);
            }
            if (departmentId > 0)
            {
                query.Where(nameof(Administrator.DepartmentId), departmentId);
            }
            if (dutyId > 0)
            {
                query.Where(nameof(Administrator.DutyId), dutyId);
            }

            return await _repository.CountAsync(query);
        }
        public async Task<int> GetCountAsync(List<int> companyIds, List<int> departmentIds, List<int> dutyIds)
        {
            var query = Q.NewQuery();
            if (companyIds != null && companyIds.Count > 0)
            {
                query.WhereIn(nameof(Administrator.CompanyId), companyIds);
            }
            if (departmentIds != null && departmentIds.Count > 0)
            {
                query.WhereIn(nameof(Administrator.DepartmentId), departmentIds);
            }
            if (dutyIds != null && dutyIds.Count > 0)
            {
                query.WhereIn(nameof(Administrator.DutyId), dutyIds);
            }

            return await _repository.CountAsync(query);
        }

        public async Task<List<Administrator>> GetListAsync(AuthorityAuth auth)
        {
            var query = Q.NewQuery();

            if (auth.AuthType == AuthorityType.Admin || auth.AuthType == AuthorityType.AdminCompany)
            {
                if (auth.CurManageOrganId != 1)
                {
                    query.WhereIn(nameof(Administrator.CompanyId), auth.CurManageOrganIds);
                }
            }
            else if (auth.AuthType == Enums.AuthorityType.AdminDepartment)
            {
                query.WhereIn(nameof(Administrator.DepartmentId), auth.CurManageOrganIds);
            }
            else
            {
                query.Where(nameof(ExamTmTree.CreatorId), auth.AdminId);
            }

            query.WhereNullOrFalse(nameof(Administrator.Locked));
            query.OrderByDesc(nameof(Administrator.Id));

            var dbs = await _repository.GetAllAsync(query);
            var list = new List<Administrator>();

            if (dbs != null)
            {
                foreach (var admin in dbs)
                {
                    if (admin != null)
                    {
                        list.Add(admin);
                    }
                }
            }

            return list;
        }
        public static Query GetQuery(AuthorityAuth auth, List<int> organIds, string organType, string role, int lastActivityDate, string keyword)
        {
            var query = Q.NewQuery();

            if (!string.IsNullOrEmpty(organType) && organIds != null && organIds.Count > 0)
            {
                if (organType == "company")
                {
                    if (!organIds.Contains(1))
                    {
                        query.WhereIn(nameof(Administrator.CompanyId), organIds);
                    }
                }
                if (organType == "department")
                {
                    query.WhereIn(nameof(Administrator.DepartmentId), organIds);
                }
                if (organType == "duty")
                {
                    query.WhereIn(nameof(Administrator.DutyId), organIds);
                }
            }


            if (auth.AuthType == AuthorityType.Admin || auth.AuthType == AuthorityType.AdminCompany)
            {
                if (auth.CurManageOrganId != 1)
                {
                    query.WhereIn(nameof(Administrator.CompanyId), auth.CurManageOrganIds);
                }
            }
            else if (auth.AuthType == Enums.AuthorityType.AdminDepartment)
            {
                query.WhereIn(nameof(Administrator.DepartmentId), auth.CurManageOrganIds);
            }
            else
            {
                query.Where(nameof(ExamTmTree.CreatorId), auth.AdminId);
            }


            if (lastActivityDate > 0)
            {
                var dateTime = DateTime.Now.AddDays(-lastActivityDate);
                query.Where(nameof(Administrator.LastActivityDate), ">=", DateUtils.ToString(dateTime));
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                var like = $"%{keyword}%";
                query.Where(q => q
                    .WhereLike(nameof(Administrator.UserName), like)
                    .OrWhereLike(nameof(Administrator.Mobile), like)
                    .OrWhereLike(nameof(Administrator.Email), like)
                    .OrWhereLike(nameof(Administrator.DisplayName), like)
                );
            }
            if (!string.IsNullOrEmpty(role))
            {
                query.Where(nameof(Administrator.Auth), role);
            }
            return query;
        }
        public async Task<int> GetCountAsync(AuthorityAuth auth, List<int> organIds, string organType, string role, int lastActivityDate, string keyword)
        {
            var query = GetQuery(auth, organIds, organType, role, lastActivityDate, keyword);

            return await _repository.CountAsync(query);
        }
        public async Task<List<Administrator>> GetListAsync(AuthorityAuth auth, List<int> organIds, string organType, string role, string order, int lastActivityDate, string keyword, int offset, int limit)
        {
            var query = GetQuery(auth, organIds, organType, role, lastActivityDate, keyword);

            if (!string.IsNullOrEmpty(order))
            {
                if (StringUtils.EqualsIgnoreCase(order, nameof(Administrator.UserName)))
                {
                    query.OrderBy(nameof(Administrator.UserName));
                }
                else
                {
                    query.OrderByDesc(order);
                }
            }
            else
            {
                query.OrderByDesc(nameof(Administrator.Id));
            }

            query.Offset(offset).Limit(limit);

            var dbs = await _repository.GetAllAsync(query);
            var list = new List<Administrator>();

            if (dbs != null)
            {
                foreach (var admin in dbs)
                {
                    if (admin != null)
                    {
                        list.Add(admin);
                    }
                }
            }

            return list;
        }

        public async Task<List<int>> GetAdministratorIdsAsync(string keyword)
        {
            var query = Q.Select(nameof(Administrator.Id));
            if (!string.IsNullOrEmpty(keyword))
            {
                var like = $"%{keyword}%";
                query.Where(q => q
                    .WhereLike(nameof(Administrator.UserName), like)
                    .OrWhereLike(nameof(Administrator.Mobile), like)
                    .OrWhereLike(nameof(Administrator.Email), like)
                    .OrWhereLike(nameof(Administrator.DisplayName), like)
                );
            }

            return await _repository.GetAllAsync<int>(query);
        }

        public async Task<List<int>> GetAdministratorIdsAsync(int companyId)
        {
            var query = Q.Select(nameof(Administrator.Id)).Where(nameof(Administrator.CompanyId), companyId);

            return await _repository.GetAllAsync<int>(query);
        }

        public async Task<bool> IsUserNameExistsAsync(string adminName)
        {
            return await _repository.ExistsAsync(Q.Where(nameof(Administrator.UserName), adminName));
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _repository.ExistsAsync(Q.Where(nameof(Administrator.Email), email));
        }

        public async Task<bool> IsMobileExistsAsync(string mobile)
        {
            return await _repository.ExistsAsync(Q.Where(nameof(Administrator.Mobile), mobile));
        }

        private string EncodePassword(string password, PasswordFormat passwordFormat, out string passwordSalt)
        {
            var retVal = string.Empty;
            passwordSalt = string.Empty;

            if (passwordFormat == PasswordFormat.Clear)
            {
                retVal = password;
            }
            else if (passwordFormat == PasswordFormat.Hashed)
            {
                passwordSalt = GenerateSalt();

                var src = Encoding.Unicode.GetBytes(password);
                var buffer2 = Convert.FromBase64String(passwordSalt);
                var dst = new byte[buffer2.Length + src.Length];
                Buffer.BlockCopy(buffer2, 0, dst, 0, buffer2.Length);
                Buffer.BlockCopy(src, 0, dst, buffer2.Length, src.Length);
                var algorithm = SHA1.Create(); // HashAlgorithm.Create("SHA1");
                if (algorithm == null) return retVal;
                var inArray = algorithm.ComputeHash(dst);

                retVal = Convert.ToBase64String(inArray);
            }
            else if (passwordFormat == PasswordFormat.Encrypted)
            {
                passwordSalt = GenerateSalt();

                retVal = TranslateUtils.EncryptStringBySecretKey(password, passwordSalt);

                //var des = new DesEncryptor
                //{
                //    InputString = password,
                //    EncryptKey = passwordSalt
                //};
                //des.DesEncrypt();

                //retVal = des.OutString;
            }
            return retVal;
        }

        private static string GenerateSalt()
        {
            // var data = new byte[0x10];
            // new RNGCryptoServiceProvider().GetBytes(data);
            var data = new byte[0x10];
            var rand = RandomNumberGenerator.Create();
            rand.GetBytes(data);
            return Convert.ToBase64String(data);
        }

        private async Task<(bool IsValid, string ErrorMessage)> UpdateValidateAsync(Administrator adminEntityToUpdate, string userName, string email, string mobile)
        {
            if (adminEntityToUpdate.UserName != null && adminEntityToUpdate.UserName != userName)
            {
                if (string.IsNullOrEmpty(adminEntityToUpdate.UserName))
                {
                    return (false, "用户名不能为空");
                }
                var config = await _configRepository.GetAsync();
                if (adminEntityToUpdate.UserName.Length < config.AdminUserNameMinLength)
                {
                    return (false, $"用户名长度必须大于等于{config.AdminUserNameMinLength}");
                }
                if (await IsUserNameExistsAsync(adminEntityToUpdate.UserName))
                {
                    return (false, "用户名已存在，请更换用户名");
                }
            }

            if (adminEntityToUpdate.Mobile != null && adminEntityToUpdate.Mobile != mobile)
            {
                if (!string.IsNullOrEmpty(adminEntityToUpdate.Mobile) && await IsMobileExistsAsync(adminEntityToUpdate.Mobile))
                {
                    return (false, "手机号码已被注册，请更换手机号码");
                }
            }

            if (adminEntityToUpdate.Email != null && adminEntityToUpdate.Email != email)
            {
                if (!string.IsNullOrEmpty(adminEntityToUpdate.Email) && await IsEmailExistsAsync(adminEntityToUpdate.Email))
                {
                    return (false, "电子邮件地址已被注册，请更换邮箱");
                }
            }

            return (true, string.Empty);
        }

        public async Task<(bool IsValid, string ErrorMessage)> InsertValidateAsync(string userName, string password, string email, string mobile)
        {
            var config = await _configRepository.GetAsync();

            if (string.IsNullOrEmpty(userName))
            {
                return (false, "用户名不能为空");
            }
            if (userName.Length < config.AdminUserNameMinLength)
            {
                return (false, $"用户名长度必须大于等于{config.AdminUserNameMinLength}");
            }
            if (string.IsNullOrEmpty(password))
            {
                return (false, "密码不能为空");
            }
            if (password.Length < config.AdminPasswordMinLength)
            {
                return (false, $"密码长度必须大于等于{config.AdminPasswordMinLength}");
            }
            if (
                !PasswordRestrictionUtils.IsValid(password,
                    config.AdminPasswordRestriction))
            {
                return (false, $"密码不符合规则，请包含{config.AdminPasswordRestriction.GetDisplayName()}");
            }

            return (true, string.Empty);
        }

        public async Task<(bool IsValid, string ErrorMessage)> InsertAsync(Administrator administrator, string password)
        {
            var valid = await InsertValidateAsync(administrator.UserName, password, administrator.Email, administrator.Mobile);
            if (!valid.IsValid) return valid;

            if (await IsUserNameExistsAsync(administrator.UserName))
            {
                return (false, "用户名已存在，请更换用户名");
            }
            if (!string.IsNullOrEmpty(administrator.Email) && await IsEmailExistsAsync(administrator.Email))
            {
                return (false, "电子邮件地址已被注册，请更换邮箱");
            }
            if (!string.IsNullOrEmpty(administrator.Mobile) && await IsMobileExistsAsync(administrator.Mobile))
            {
                return (false, "手机号码已被注册，请更换手机号码");
            }

            try
            {
                administrator.LastActivityDate = DateTime.Now;
                administrator.LastChangePasswordDate = DateTime.Now;
                administrator.PasswordFormat = PasswordFormat.Encrypted;
                administrator.Password = EncodePassword(password, administrator.PasswordFormat, out var passwordSalt);
                administrator.PasswordSalt = passwordSalt;
                administrator.Set("ConfirmPassword", string.Empty);

                await _repository.InsertAsync(administrator);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool IsValid, string ErrorMessage)> UpdateAsync(Administrator administrator)
        {
            var admin = await GetByUserIdAsync(administrator.Id);
            var cacheKeys = GetCacheKeys(admin);

            var valid = await UpdateValidateAsync(admin, administrator.UserName, administrator.Email, administrator.Mobile);
            if (!valid.IsValid) return valid;

            await _repository.UpdateAsync(Q
                .Set(nameof(Administrator.LastActivityDate), administrator.LastActivityDate)
                .Set(nameof(Administrator.LastChangePasswordDate), administrator.LastChangePasswordDate)
                .Set(nameof(Administrator.CountOfLogin), administrator.CountOfLogin)
                .Set(nameof(Administrator.CountOfFailedLogin), administrator.CountOfFailedLogin)
                .Set(nameof(Administrator.Locked), administrator.Locked)
                .Set(nameof(Administrator.DisplayName), administrator.DisplayName)
                .Set(nameof(Administrator.Mobile), administrator.Mobile)
                .Set(nameof(Administrator.Email), administrator.Email)
                .Set(nameof(Administrator.AvatarUrl), administrator.AvatarUrl)
                .Set(nameof(Administrator.UserName), administrator.UserName)
                .Set(nameof(Administrator.Auth), administrator.Auth)
                .Set(nameof(Administrator.CompanyId), administrator.CompanyId)
                .Set(nameof(Administrator.DepartmentId), administrator.DepartmentId)
                .Set(nameof(Administrator.DutyId), administrator.DutyId)
                .Set(nameof(Administrator.AuthManageOrganId), administrator.AuthManageOrganId)
                .Set(nameof(Administrator.AuthCurManageOrganId), administrator.AuthCurManageOrganId)
                .Where(nameof(Administrator.Id), administrator.Id)
                .CachingRemove(cacheKeys.ToArray())
            );

            return (true, string.Empty);
        }

        public async Task<(bool IsValid, string ErrorMessage)> ChangePasswordAsync(Administrator adminEntity, string password)
        {
            var config = await _configRepository.GetAsync();

            if (string.IsNullOrEmpty(password))
            {
                return (false, "密码不能为空");
            }
            if (password.Length < config.AdminPasswordMinLength)
            {
                return (false, $"密码长度必须大于等于{config.AdminPasswordMinLength}");
            }
            if (
                !PasswordRestrictionUtils.IsValid(password, config.AdminPasswordRestriction))
            {
                return (false, $"密码不符合规则，请包含{config.AdminPasswordRestriction.GetDisplayName()}");
            }

            password = EncodePassword(password, PasswordFormat.Encrypted, out var passwordSalt);
            await ChangePasswordAsync(adminEntity, PasswordFormat.Encrypted, passwordSalt, password);
            return (true, string.Empty);
        }

        public async Task<(Administrator administrator, string userName, string errorMessage)> ValidateAsync(string account, string password, bool isPasswordMd5)
        {
            var userName = string.Empty;

            if (string.IsNullOrEmpty(account))
            {
                return (null, userName, "账号不能为空");
            }
            if (string.IsNullOrEmpty(password))
            {
                return (null, userName, "密码不能为空");
            }

            var administrator = await GetByAccountAsync(account);
            if (string.IsNullOrEmpty(administrator?.UserName))
            {
                return (null, userName, "帐号或密码错误");
            }

            userName = administrator.UserName;

            var (success, errorMessage) = await ValidateLockAsync(administrator);
            if (!success)
            {
                return (null, userName, errorMessage);
            }

            return await CheckPasswordByAdminIdAsync(administrator.Id, password, isPasswordMd5)
              ? (administrator, userName, string.Empty)
              : (null, userName, "账号或密码错误");
        }
        private async Task<bool> CheckPasswordByAdminIdAsync(int adminId, string password, bool isPasswordMd5)
        {
            var dbAdmin = await _repository.GetAsync<Administrator>(Q
                .Select(nameof(Administrator.Password))
                .Select(nameof(Administrator.PasswordFormat))
                .Select(nameof(Administrator.PasswordSalt))
                .Where(nameof(Administrator.Id), adminId)
            );

            if (dbAdmin == null || string.IsNullOrEmpty(dbAdmin.Password) || string.IsNullOrEmpty(dbAdmin.PasswordSalt))
            {
                return false;
            }

            var decodePassword = DecodePassword(dbAdmin.Password, dbAdmin.PasswordFormat, dbAdmin.PasswordSalt);
            if (isPasswordMd5)
            {
                return password == AuthUtils.Md5ByString(decodePassword);
            }
            return password == decodePassword;
        }
        public async Task<(bool Success, string ErrorMessage)> ValidateLockAsync(Administrator administrator)
        {
            if (administrator.Locked)
            {
                return (false, "此账号被锁定，无法登录");
            }

            var config = await _configRepository.GetAsync();

            if (config.IsAdminLockLogin)
            {
                if (administrator.CountOfFailedLogin > 0 &&
                    administrator.CountOfFailedLogin >= config.AdminLockLoginCount)
                {
                    var lockType = config.AdminLockLoginType;
                    if (lockType == LockType.Forever)
                    {
                        return (false, "此账号错误登录次数过多，已被永久锁定");
                    }
                    if (lockType == LockType.Hours && administrator.LastActivityDate.HasValue)
                    {
                        var ts = new TimeSpan(DateTime.Now.Ticks - administrator.LastActivityDate.Value.Ticks);
                        var hours = Convert.ToInt32(config.AdminLockLoginHours - ts.TotalHours);
                        if (hours > 0)
                        {
                            return (false, $"此账号错误登录次数过多，已被锁定，请等待{hours}小时后重试");
                        }
                    }
                }
            }

            return (true, null);
        }

        private static string DecodePassword(string password, PasswordFormat passwordFormat, string passwordSalt)
        {
            var retVal = string.Empty;
            if (passwordFormat == PasswordFormat.Clear)
            {
                retVal = password;
            }
            else if (passwordFormat == PasswordFormat.Hashed)
            {
                throw new Exception("can not decode hashed password");
            }
            else if (passwordFormat == PasswordFormat.Encrypted)
            {
                retVal = TranslateUtils.DecryptStringBySecretKey(password, passwordSalt);

                //var des = new DesEncryptor
                //{
                //    InputString = password,
                //    DecryptKey = passwordSalt
                //};
                //des.DesDecrypt();

                //retVal = des.OutString;
            }
            return retVal;
        }
        public async Task<Administrator> DeleteAsync(int id)
        {
            var admin = await GetByUserIdAsync(id);
            var cacheKeys = GetCacheKeys(admin);

            await _repository.DeleteAsync(id, Q.CachingRemove(cacheKeys.ToArray()));

            return admin;
        }
        public async Task DeleteByCompanyIdAsync(int companyId)
        {
            var adminIds = await GetAdministratorIdsAsync(companyId);
            if (adminIds != null && adminIds.Count > 0)
            {
                foreach (var adminId in adminIds)
                {
                    var admin = await GetByUserIdAsync(adminId);
                    var cacheKeys = GetCacheKeys(admin);
                    await _repository.DeleteAsync(adminId, Q.CachingRemove(cacheKeys.ToArray()));
                }
            }

        }
        public async Task ClearAsync()
        {
            await _repository.DeleteAsync(Q.WhereNot(nameof(OrganCompany.Id), 1));
        }
    }
}

﻿using Datory;
using System.Collections.Generic;
using System.Threading.Tasks;
using XBLMS.Dto;
using XBLMS.Models;
using XBLMS.Repositories;
using XBLMS.Services;

namespace XBLMS.Core.Repositories
{
    public class StudyCourseEvaluationRepository : IStudyCourseEvaluationRepository
    {
        private readonly ISettingsManager _settingsManager;
        private readonly Repository<StudyCourseEvaluation> _repository;

        public StudyCourseEvaluationRepository(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            _repository = new Repository<StudyCourseEvaluation>(settingsManager.Database, settingsManager.Redis);
        }

        public IDatabase Database => _repository.Database;

        public string TableName => _repository.TableName;

        public List<TableColumn> TableColumns => _repository.TableColumns;

        public async Task<int> InsertAsync(StudyCourseEvaluation item)
        {
            return await _repository.InsertAsync(item);
        }

        public async Task<bool> UpdateAsync(StudyCourseEvaluation item)
        {
            return await _repository.UpdateAsync(item);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<StudyCourseEvaluation> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }


        public async Task<(int total, List<StudyCourseEvaluation> list)> GetListAsync(AuthorityAuth auth, string keyWords, int pageIndex, int pageSize)
        {
            var query = Q.NewQuery();

            if (auth.AuthType == Enums.AuthorityType.Admin || auth.AuthType == Enums.AuthorityType.AdminCompany)
            {
                query.Where(nameof(StudyCourseEvaluation.CompanyId), auth.CurManageOrganId);
            }
            else if (auth.AuthType == Enums.AuthorityType.AdminDepartment)
            {
                query.Where(nameof(StudyCourseEvaluation.DepartmentId), auth.CurManageOrganId);
            }
            else
            {
                query.Where(nameof(StudyCourseEvaluation.CreatorId), auth.AdminId);
            }


            if (!string.IsNullOrEmpty(keyWords))
            {
                query.WhereLike(nameof(StudyCourseEvaluation.Title), $"%{keyWords}%");
            }
            query.OrderByDesc(nameof(StudyCourseEvaluation.Id));

            var total = await _repository.CountAsync(query);
            var list = await _repository.GetAllAsync(query.ForPage(pageIndex, pageSize));
            return (total, list);
        }


        public async Task<int> MaxAsync()
        {
            var maxId = await _repository.MaxAsync(nameof(StudyCourseEvaluation.Id));
            if (maxId.HasValue)
            {
                return maxId.Value + 1;
            }
            return 1;
        }

        public async Task<(int allCount, int addCount, int deleteCount, int lockedCount, int unLockedCount)> GetDataCount(AuthorityAuth auth)
        {
            var total = 0;
            var lockedTotal = 0;
            var unLockedTotal = 0;
            if (auth.AuthType == Enums.AuthorityType.Admin || auth.AuthType == Enums.AuthorityType.AdminCompany)
            {
                total = await _repository.CountAsync(Q.WhereIn(nameof(StudyCourseEvaluation.CompanyId), auth.CurManageOrganIds));
                lockedTotal = await _repository.CountAsync(Q.WhereIn(nameof(StudyCourseEvaluation.CompanyId), auth.CurManageOrganIds).WhereTrue(nameof(StudyCourseEvaluation.Locked)));
                unLockedTotal = await _repository.CountAsync(Q.WhereIn(nameof(StudyCourseEvaluation.CompanyId), auth.CurManageOrganIds).WhereNullOrFalse(nameof(StudyCourseEvaluation.Locked)));
            }
            else if (auth.AuthType == Enums.AuthorityType.AdminDepartment)
            {
                total = await _repository.CountAsync(Q.WhereIn(nameof(StudyCourseEvaluation.DepartmentId), auth.CurManageOrganIds));
                lockedTotal = await _repository.CountAsync(Q.WhereIn(nameof(StudyCourseEvaluation.DepartmentId), auth.CurManageOrganIds).WhereTrue(nameof(StudyCourseEvaluation.Locked)));
                unLockedTotal = await _repository.CountAsync(Q.WhereIn(nameof(StudyCourseEvaluation.DepartmentId), auth.CurManageOrganIds).WhereNullOrFalse(nameof(StudyCourseEvaluation.Locked)));
            }
            else
            {
                total = await _repository.CountAsync(Q.Where(nameof(StudyCourseEvaluation.CreatorId), auth.AdminId));
                lockedTotal = await _repository.CountAsync(Q.Where(nameof(StudyCourseEvaluation.CreatorId), auth.AdminId).WhereTrue(nameof(StudyCourseEvaluation.Locked)));
                unLockedTotal = await _repository.CountAsync(Q.Where(nameof(StudyCourseEvaluation.CreatorId), auth.AdminId).WhereNullOrFalse(nameof(StudyCourseEvaluation.Locked)));
            }

            return (total, 0, 0, lockedTotal, unLockedTotal);
        }
    }
}

﻿using Datory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XBLMS.Dto;
using XBLMS.Models;
using XBLMS.Repositories;
using XBLMS.Services;

namespace XBLMS.Core.Repositories
{
    public class StudyPlanRepository : IStudyPlanRepository
    {
        private readonly ISettingsManager _settingsManager;
        private readonly Repository<StudyPlan> _repository;

        public StudyPlanRepository(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            _repository = new Repository<StudyPlan>(settingsManager.Database, settingsManager.Redis);
        }

        public IDatabase Database => _repository.Database;

        public string TableName => _repository.TableName;

        public List<TableColumn> TableColumns => _repository.TableColumns;

        public async Task<int> InsertAsync(StudyPlan item)
        {
            return await _repository.InsertAsync(item);
        }

        public async Task<bool> UpdateAsync(StudyPlan item)
        {
            return await _repository.UpdateAsync(item);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<StudyPlan> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }


        public async Task<(int total, List<StudyPlan> list)> GetListAsync(AuthorityAuth auth, string keyWords, int pageIndex, int pageSize)
        {
            var query = Q.NewQuery();

            if (auth.AuthType == Enums.AuthorityType.Admin || auth.AuthType == Enums.AuthorityType.AdminCompany)
            {
                query.Where(nameof(StudyPlan.CompanyId), auth.CurManageOrganId);
            }
            else if (auth.AuthType == Enums.AuthorityType.AdminDepartment)
            {
                query.Where(nameof(StudyPlan.DepartmentId), auth.CurManageOrganId);
            }
            else
            {
                query.Where(nameof(StudyPlan.CreatorId), auth.AdminId);
            }

            if (!string.IsNullOrEmpty(keyWords))
            {
                query.Where(q => q.WhereLike(nameof(StudyPlan.PlanName), $"%{keyWords}%").OrWhereLike(nameof(StudyPlan.Description), $"%{keyWords}%"));
            }
            query.OrderByDesc(nameof(StudyPlan.Id));
            var total = await _repository.CountAsync(query);
            var list = await _repository.GetAllAsync(query.ForPage(pageIndex, pageSize));
            return (total, list);
        }


        public async Task<int> MaxAsync()
        {
            var maxId = await _repository.MaxAsync(nameof(StudyPlan.Id));
            if (maxId.HasValue)
            {
                return maxId.Value + 1;
            }
            return 1;
        }


        public async Task<List<int>> GetYearListAsync()
        {
            var query = Q.Select(nameof(StudyPlan.PlanYear));
            var list = await _repository.GetAllAsync<int>(query);
            if (list != null && list.Count > 0)
            {
                return list.Distinct().ToList();
            }
            return null;
        }
    }
}

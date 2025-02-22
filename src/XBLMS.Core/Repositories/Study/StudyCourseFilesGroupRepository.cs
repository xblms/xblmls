﻿using Datory;
using System.Collections.Generic;
using System.Threading.Tasks;
using XBLMS.Dto;
using XBLMS.Models;
using XBLMS.Repositories;
using XBLMS.Services;

namespace XBLMS.Core.Repositories
{
    public class StudyCourseFilesGroupRepository : IStudyCourseFilesGroupRepository
    {
        private readonly Repository<StudyCourseFilesGroup> _repository;

        public StudyCourseFilesGroupRepository(ISettingsManager settingsManager)
        {
            _repository = new Repository<StudyCourseFilesGroup>(settingsManager.Database, settingsManager.Redis);
        }

        public IDatabase Database => _repository.Database;

        public string TableName => _repository.TableName;

        public List<TableColumn> TableColumns => _repository.TableColumns;

        public async Task<int> InsertAsync(StudyCourseFilesGroup group)
        {
            return await _repository.InsertAsync(group);
        }

        public async Task<bool> UpdateAsync(StudyCourseFilesGroup group)
        {
            return await _repository.UpdateAsync(group);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<StudyCourseFilesGroup> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public async Task<bool> IsExistsAsync(int parentId, string groupName)
        {
            return await _repository.ExistsAsync(Q
                .Where(nameof(StudyCourseFilesGroup.ParentId), parentId)
                .Where(nameof(StudyCourseFilesGroup.GroupName), groupName));
        }


        public async Task<List<StudyCourseFilesGroup>> GetListAsync(AuthorityAuth auth, int parentId)
        {
            var query = Q.NewQuery();


            if (auth.AuthType == Enums.AuthorityType.Admin || auth.AuthType == Enums.AuthorityType.AdminCompany)
            {
                query.Where(nameof(StudyCourseFilesGroup.CompanyId), auth.CurManageOrganId);
            }
            else if (auth.AuthType == Enums.AuthorityType.AdminDepartment)
            {
                query.Where(nameof(StudyCourseFilesGroup.DepartmentId), auth.CurManageOrganId);
            }
            else
            {
                query.Where(nameof(StudyCourseFilesGroup.CreatorId), auth.AdminId);
            }


            return await _repository.GetAllAsync(Q.
                Where(nameof(StudyCourseFilesGroup.ParentId), parentId).
                OrderBy(nameof(StudyCourseFilesGroup.CreatedDate)));
        }

        public async Task<List<int>> GetParentIdListAsync(int id)
        {
            var ids = new List<int>();
            ids.Insert(0, id);

            var group = await GetAsync(id);
            await GetParentIdListAsync(ids, group);

            return ids;
        }
        private async Task GetParentIdListAsync(List<int> parentIds, StudyCourseFilesGroup group)
        {
            if (group != null)
            {
                if (group.ParentId > 0)
                {
                    var parent = await GetAsync(group.ParentId);
                    if (parent != null)
                    {
                        parentIds.Insert(0, parent.Id);
                        await GetParentIdListAsync(parentIds, parent);
                    }
                }
            }

        }


        public async Task<List<int>> GetChildIdListAsync(int id)
        {
            var ids = new List<int>();
            ids.Add(id);

            var group = await GetAsync(id);
            await GetChildIdListAsync(ids, id);

            return ids;
        }
        private async Task GetChildIdListAsync(List<int> childIds, int parentId)
        {
            if (parentId > 0)
            {
                var childIdList = await _repository.GetAllAsync<int>(Q.Select(nameof(StudyCourseFilesGroup.Id)).Where(nameof(StudyCourseFilesGroup.ParentId), parentId));
                if (childIdList != null && childIdList.Count > 0)
                {
                    foreach (var childId in childIdList)
                    {
                        childIds.Add(childId);
                        await GetChildIdListAsync(childIds, childId);
                    }
                }
            }
        }
    }
}

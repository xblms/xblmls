using Datory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XBLMS.Core.Utils;
using XBLMS.Models;
using XBLMS.Repositories;
using XBLMS.Services;

namespace XBLMS.Core.Repositories
{
    public class ExamCerUserRepository : IExamCerUserRepository
    {
        private readonly Repository<ExamCerUser> _repository;

        public ExamCerUserRepository(ISettingsManager settingsManager)
        {
            _repository = new Repository<ExamCerUser>(settingsManager.Database, settingsManager.Redis);
        }

        public IDatabase Database => _repository.Database;

        public string TableName => _repository.TableName;

        public List<TableColumn> TableColumns => _repository.TableColumns;


        public async Task<bool> ExistsAsync(int userID, int examPaperId)
        {
            return await _repository.ExistsAsync(Q.Where(nameof(ExamCerUser.UserId), userID).Where(nameof(ExamCerUser.ExamPaperId), examPaperId));
        }
        public async Task<bool> ExistsAsync(int userID, int examPaperId, int planId, int courseId)
        {
            return await _repository.ExistsAsync(Q.
                Where(nameof(ExamCerUser.UserId), userID).
                Where(nameof(ExamCerUser.PlanId), planId).
                Where(nameof(ExamCerUser.CourseId), courseId).
                Where(nameof(ExamCerUser.ExamPaperId), examPaperId));
        }
        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
        public async Task<bool> ClearAsync(int userId)
        {
            return await _repository.DeleteAsync(Q.Where(nameof(ExamCerUser.UserId), userId)) > 0;
        }
        public async Task<bool> ClearByPaperAsync(int examPaperId)
        {
            return await _repository.DeleteAsync(Q.Where(nameof(ExamCerUser.ExamPaperId), examPaperId)) > 0;
        }
        public async Task<ExamCerUser> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }
        public async Task<ExamCerUser> GetAsync(int userId, int examPaperId)
        {
            return await _repository.GetAsync(Q.Where(nameof(ExamCerUser.UserId), userId).Where(nameof(ExamCerUser.ExamPaperId), examPaperId));
        }
        public async Task<List<ExamCerUser>> GetListAsync(int examPaperId)
        {
            var list = (await _repository.GetAllAsync(Q
                .Where(nameof(ExamCerUser.ExamPaperId), examPaperId)
                .OrderBy(nameof(ExamCerUser.Id))
            )).ToList();
            return list;
        }
        public async Task<List<ExamCerUser>> GetListAsync()
        {
            var list = (await _repository.GetAllAsync(Q
                .OrderBy(nameof(ExamCerUser.Id))
            )).ToList();
            return list;
        }

        public async Task<int> InsertAsync(ExamCerUser item)
        {
            return await _repository.InsertAsync(item);
        }
        public async Task<(int total, List<ExamCerUser> list)> GetListAsync(int userId, int pageIndex, int pageSize)
        {
            var query = Q
                .Where(nameof(ExamCerUser.UserId), userId)
                .OrderByDesc(nameof(ExamCerUser.Id));
            var total = await _repository.CountAsync(query);
            var list = await _repository.GetAllAsync(query.ForPage(pageIndex, pageSize));
            return (total, list);
        }
        public async Task<int> GetCountAsync(int cerId)
        {

            var query = Q.Where(nameof(ExamCerUser.CerId), cerId);
            var total = await _repository.CountAsync(query);
            return total;
        }
        public async Task<int> UpdateImgAsync(int id, string img)
        {
            return await _repository.UpdateAsync(Q.Set(nameof(ExamCerUser.CerImg), img).Where(nameof(ExamCerUser.Id), id));
        }



        public async Task<(int total, List<ExamCerUser> list)> GetListAsync(int cerId, string keyWords, string beginDate, string endDate, int pageIndex, int pageSize)
        {
            var query = Q.Where(nameof(ExamCerUser.CerId), cerId);

            if (!string.IsNullOrEmpty(keyWords))
            {
                keyWords = $"%{keyWords}%";
                query.WhereLike(nameof(ExamCerUser.KeyWordsAdmin), keyWords);
            }
            if (!string.IsNullOrEmpty(beginDate))
            {
                query.Where(nameof(ExamCerUser.CreatedDate), ">=", DateUtils.ToString(beginDate));
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                query.Where(nameof(ExamCerUser.CreatedDate), "<=", DateUtils.ToString(endDate));
            }
            query.OrderByDesc(nameof(ExamCerUser.Id));

            var total = await _repository.CountAsync(query);
            var list = await _repository.GetAllAsync(query.ForPage(pageIndex, pageSize));
            return (total, list);
        }

        public async Task<(int total, List<ExamCerUser> list)> GetListAsync(int cerId,int planId,int courseId, string keyWords, string beginDate, string endDate, int pageIndex, int pageSize)
        {
            var query = Q.Where(nameof(ExamCerUser.CerId), cerId);

            if (planId > 0)
            {
                query.Where(nameof(ExamCerUser.PlanId), planId);
            }
            if (courseId > 0)
            {
                query.Where(nameof(ExamCerUser.CourseId),courseId);
            }

            if (!string.IsNullOrEmpty(keyWords))
            {
                keyWords = $"%{keyWords}%";
                query.WhereLike(nameof(ExamCerUser.KeyWordsAdmin), keyWords);
            }
            if (!string.IsNullOrEmpty(beginDate))
            {
                query.Where(nameof(ExamCerUser.CreatedDate), ">=", DateUtils.ToString(beginDate));
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                query.Where(nameof(ExamCerUser.CreatedDate), "<=", DateUtils.ToString(endDate));
            }
            query.OrderByDesc(nameof(ExamCerUser.Id));

            var total = await _repository.CountAsync(query);
            var list = await _repository.GetAllAsync(query.ForPage(pageIndex, pageSize));
            return (total, list);
        }
    }

}

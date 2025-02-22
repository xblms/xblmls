using Datory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XBLMS.Models;

namespace XBLMS.Repositories
{
    public interface IExamQuestionnaireUserRepository : IRepository
    {
        Task<ExamQuestionnaireUser> GetAsync(int paperId, int userId, int planId, int courseId);
        Task<ExamQuestionnaireUser> GetAsync(int paperId, int userId);
        Task<ExamQuestionnaireUser> GetOnlyOneAsync(int userId);
        Task ClearByUserAsync(int userId);
        Task ClearByPaperAsync(int paperId);
        Task ClearByPaperAndUserAsync(int paperId, int userId);
        Task DeleteAsync(int id);
        Task<int> InsertAsync(ExamQuestionnaireUser item);
        Task<bool> UpdateAsync(ExamQuestionnaireUser item);
        Task<bool> ExistsAsync(int paperId, int userId);
        Task<int> GetSubmitUserCountAsync(int planId, int courseId, int paperId);
        Task<List<int>> GetPaperIdsAsync(int userId);
        Task<(int total, List<ExamQuestionnaireUser> list)> GetListAsync(int userId, string keyWords, int pageIndex, int pageSize);
        Task<(int total, List<ExamQuestionnaireUser> list)> GetListAsync(int paperId,string isSubmit, string keyWords, int pageIndex, int pageSize);
        Task UpdateLockedAsync(int paperId, bool locked);
        Task UpdateKeyWordsAsync(int paperId, string keyWords);
        Task UpdateKeyWordsAdminAsync(int id, string keyWords);
        Task UpdateExamDateTimeAsync(int paperId, DateTime beginDateTime, DateTime endDateTime);

        Task<int> GetCountByTaskAsync(int userId);
    }
}

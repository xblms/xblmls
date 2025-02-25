using Datory;
using System.Collections.Generic;
using System.Threading.Tasks;
using XBLMS.Models;

namespace XBLMS.Repositories
{
    public  interface IExamQuestionnaireAnswerRepository : IRepository
    {
        Task<int> InsertAsync(ExamQuestionnaireAnswer item);
        Task<bool> UpdateAsync(ExamQuestionnaireAnswer item);
        Task ClearByUserAsync(int userId);
        Task ClearByPaperAsync(int paperId);
        Task ClearByPaperAndUserAsync(int paperId, int userId);
        Task DeleteAsync(int id);
        Task<int> GetCountSubmitUser(int paperId, int tmId, string answer);
        Task<List<string>> GetListAnswer(int planId, int courseId, int paperId, int tmId);
        Task<int> GetCountSubmitUser(int planId, int courseId, int paperId, int tmId, string answer);
        Task<List<string>> GetListAnswer(int paperId, int tmId);
 
    }
}

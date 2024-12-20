﻿using System.Collections.Generic;
using System.Threading.Tasks;
using XBLMS.Dto;
using XBLMS.Models;

namespace XBLMS.Repositories
{
    public partial interface IExamPaperTreeRepository
    {
        Task<List<ExamPaperTree>> GetListAsync(AuthorityAuth auth);
        Task<string> GetPathNamesAsync(int id);
        Task<List<ExamPaperTree>> GetListAsync();
        Task<ExamPaperTree> GetAsync(int id);
        Task<List<int>> GetIdsAsync(int id);
    }
}

﻿using System.Collections.Generic;
using System.Threading.Tasks;
using XBLMS.Dto;
using XBLMS.Enums;
using XBLMS.Models;

namespace XBLMS.Services
{
    public partial interface IStudyManager
    {
        Task<List<Cascade<int>>> GetStudyCourseTreeCascadesAsync(AuthorityAuth auth);
    }

}

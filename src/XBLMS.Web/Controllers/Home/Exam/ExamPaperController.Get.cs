﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using XBLMS.Dto;
using XBLMS.Models;

namespace XBLMS.Web.Controllers.Home.Exam
{
    public partial class ExamPaperController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] GetRequest request)
        {
            var user = await _authManager.GetUserAsync();
            if (user == null) { return Unauthorized(); }

            var (total, list) = await _examPaperUserRepository.GetListAsync(user.Id, false, request.Date, request.KeyWords, request.PageIndex, request.PageSize);
            var resultList = new List<ExamPaper>();
            if (total > 0)
            {
                foreach (var item in list)
                {
                    var paper = await _examPaperRepository.GetAsync(item.ExamPaperId);
                    await _examManager.GetPaperInfo(paper, user, item.PlanId, item.CourseId);
                    resultList.Add(paper);
                }
            }
            return new GetResult
            {
                Total = total,
                List = resultList
            };
        }

        [HttpGet, Route(RouteItem)]
        public async Task<ActionResult<ItemResult<ExamPaper>>> GetItem([FromQuery] IdRequest request)
        {
            var user = await _authManager.GetUserAsync();
            if (user == null) { return Unauthorized(); }

            var paperUser = await _examPaperUserRepository.GetAsync(request.Id);
            var paper = await _examPaperRepository.GetAsync(paperUser.ExamPaperId);
            await _examManager.GetPaperInfo(paper, user, paperUser.PlanId, paperUser.CourseId);
            return new ItemResult<ExamPaper>
            {
                Item = paper
            };
        }
    }
}

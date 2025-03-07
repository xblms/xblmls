﻿using Datory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XBLMS.Dto;
using XBLMS.Models;

namespace XBLMS.Core.Services
{
    public partial class StudyManager
    {
        public async Task<List<Cascade<int>>> GetStudyCourseTreeCascadesAsync(AuthorityAuth auth)
        {
            var list = new List<Cascade<int>>();
            var trees = await _studyCourseTreeRepository.GetListAsync(auth);
            var items = trees.Where(p => p.ParentId == 0).ToList();

            if (items != null && items.Any())
            {
                foreach (var item in items)
                {
                    var total = 0;
                    var count = 0;

                    (total, count) = await _studyCourseRepository.CountAsync(item.Id);

                    var cascade = new Cascade<int>
                    {
                        Id = item.Id,
                        Popover = false,
                        Label = item.Name,
                        Value = item.Id,
                        Total = total,
                        SelfTotal = count,
                        Children = await GetStudyCourseTreeCascadesAsync(trees, item.Id)
                    };
                    list.Add(cascade);
                }
            }
            return list;
        }
        private async Task<List<Cascade<int>>> GetStudyCourseTreeCascadesAsync(List<StudyCourseTree> all, int parentId)
        {
            var list = new List<Cascade<int>>();
            var items = all.Where(p => p.ParentId == parentId).ToList();

            if (items != null && items.Count > 0)
            {
                foreach (var item in items)
                {
                    var total = 0;
                    var count = 0;

                    (total, count) = await _studyCourseRepository.CountAsync(item.Id);

                    var cascade = new Cascade<int>
                    {
                        Id = item.Id,
                        Popover = false,
                        Label = item.Name,
                        Value = item.Id,
                        SelfTotal = count,
                        Total = total,
                        Children = await GetStudyCourseTreeCascadesAsync(all, item.Id)
                    };
                    list.Add(cascade);
                }
            }
            return list;
        }
    }
}

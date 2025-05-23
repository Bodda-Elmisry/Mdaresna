﻿using Mdaresna.Doamin.DTOs.ClassRoom;
using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.Enums;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Command;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.ClassRoomManagement.Query
{
    public class ClassRoomHelpDataQueryRepository : IClassRoomHelpDataQueryRepository
    {
        private readonly AppDbContext context;

        public ClassRoomHelpDataQueryRepository(AppDbContext context)
        {
            this.context = context;
        }
        public async Task<ClassRoomHelpDataDTO> GetClassRoomHrlpDataAsync(Guid SchoolId)
        {
           var languages = await context.ClassRoomLanguages
                                  .Where(l => l.SchoolId == SchoolId && l.Deleted == false) 
                                  .Select(l=> new DropDownDTO {Id= l.Language.Id, Name = l.Language.Name }).ToListAsync();
            var grades = await context.SchoolGrades
                                .Where(g=> g.SchoolId == SchoolId && g.Deleted == false)
                                .Select (g=> new DropDownDTO{Id = g.Id, Name = g.Name}).ToListAsync();



            var result = new ClassRoomHelpDataDTO
            {
                SchoolGrades = grades,
                SchoolLanguages = languages,
                Genders = GetGenders()
            };

            return result;

        }

        private Dictionary<int,string> GetGenders()
        {
            var genders = new Dictionary<int,string>();
            foreach(var value in Enum.GetValues<ClassRoomGenderEnum>())
            {
                genders.Add((int)value,value.ToString());
            }

            return genders;
        }
    }

   
}

using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Command;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.ClassRoomManagement.Command
{
    public class ClassRoomAssignmentCommandRepository : BaseCommandRepository<ClassRoomAssignment>, IClassRoomAssignmentCommandRepository
    {
        private readonly AppDbContext context;

        public ClassRoomAssignmentCommandRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

    }
}

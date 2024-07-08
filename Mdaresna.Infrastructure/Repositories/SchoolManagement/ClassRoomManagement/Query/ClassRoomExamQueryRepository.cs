using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.Models.Base.Relation;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.ClassRoomManagement.Query
{
    public class ClassRoomExamQueryRepository : BaseQueryRepository<ClassRoomExam>, IClassRoomExamQueryRepository
    {
        private readonly AppDbContext context;

        public ClassRoomExamQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public Task<IEnumerable<ClassRoomExamResultDTO>> GetExamsList(IEnumerable<Guid> months, DateTime? fromDate, DateTime? toDate,
                                                                      string weekDay, Guid? classRoomId, Guid? supervisorId,
                                                                      Guid? courseId, decimal? rate) 
        {
            var query = context.ClassRoomExams.Where(e => months.Contains(e.MonthId));

            if (fromDate != null && toDate != null)
                query = query.Where(e => e.ExamDate >= fromDate && e.ExamDate <= toDate);
            else if(fromDate != null)
                query = query.Where(e=> e.ExamDate == fromDate);

            if(!string.IsNullOrEmpty(weekDay))
                query = query.Where(e=> e.WeekDay == weekDay);

            if(classRoomId != null)
                query = query.Where(e=> e.ClassRoomId == classRoomId);

            if(supervisorId != null)
                query = query.Where(e=> e.SupervisorId == supervisorId);



        }







    }
}

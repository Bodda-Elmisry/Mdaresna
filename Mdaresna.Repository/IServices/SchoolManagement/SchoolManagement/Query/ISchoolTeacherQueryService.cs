﻿using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query
{
    public interface ISchoolTeacherQueryService : IBaseQueryService<SchoolTeacher>
    {

        Task<bool> isExist(Guid schoolId, Guid teacherId);
        Task<IEnumerable<TeacherSchoolResultDTO>> GetTeacherSchoolsAsync(Guid teacherId);
        Task<IEnumerable<TeacherResultDTO>> GetSchoolTeachersAsync(Guid schoolId, string teacherName, string teacherPhoneNumber, string teacherEmail);
        Task<SchoolTeacher> GetSchoolTeacherByIdAsync(Guid schoolId, Guid teacherId);

    }
}

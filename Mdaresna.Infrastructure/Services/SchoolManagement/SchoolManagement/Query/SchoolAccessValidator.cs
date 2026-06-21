using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Query
{
    public class SchoolAccessValidator : ISchoolAccessValidator
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public SchoolAccessValidator(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<bool> CanAccessSchoolAsync(Guid userId, Guid schoolId)
        {
            if (userId == Guid.Empty || schoolId == Guid.Empty)
                return false;

            string cacheKey = $"school-access-{userId}-{schoolId}";
            if (_cache.TryGetValue(cacheKey, out bool hasAccess))
            {
                return hasAccess;
            }

            // 1. Is School Admin?
            bool isAdmin = await _context.Schools.AnyAsync(s => s.Id == schoolId && s.SchoolAdminId == userId && !s.Deleted);
            if (isAdmin)
            {
                _cache.Set(cacheKey, true, TimeSpan.FromMinutes(5));
                return true;
            }

            // 2. Is Active Teacher?
            bool isTeacher = await _context.schoolTeachers.AnyAsync(t => t.SchoolId == schoolId && t.TeacherId == userId && !t.Deleted);
            if (isTeacher)
            {
                _cache.Set(cacheKey, true, TimeSpan.FromMinutes(5));
                return true;
            }

            // 3. Is Active Employee?
            bool isEmployee = await _context.SchoolEmployees.AnyAsync(e => e.SchoolId == schoolId && e.EmployeeId == userId && !e.Deleted);
            if (isEmployee)
            {
                _cache.Set(cacheKey, true, TimeSpan.FromMinutes(5));
                return true;
            }

            // 4. Is Active Parent of an active student in this school?
            bool isParent = await _context.StudentParents.AnyAsync(sp => sp.ParentId == userId && sp.Student.SchoolId == schoolId && !sp.Deleted && !sp.Student.Deleted);
            if (isParent)
            {
                _cache.Set(cacheKey, true, TimeSpan.FromMinutes(5));
                return true;
            }

            _cache.Set(cacheKey, false, TimeSpan.FromMinutes(5));
            return false;
        }

        public async Task<bool> CanAccessStudentAsync(Guid userId, Guid studentId)
        {
            if (userId == Guid.Empty || studentId == Guid.Empty)
                return false;

            // Resolve student's school ID
            var schoolId = await _context.Students
                .Where(s => s.Id == studentId && !s.Deleted)
                .Select(s => s.SchoolId)
                .FirstOrDefaultAsync();

            if (schoolId == Guid.Empty)
                return false;

            return await CanAccessSchoolAsync(userId, schoolId);
        }

        public async Task<bool> CanAccessClassRoomAsync(Guid userId, Guid classRoomId)
        {
            if (userId == Guid.Empty || classRoomId == Guid.Empty)
                return false;

            // Resolve classroom's school ID
            var schoolId = await _context.ClassRooms
                .Where(c => c.Id == classRoomId && !c.Deleted)
                .Select(c => c.SchoolId)
                .FirstOrDefaultAsync();

            if (schoolId == Guid.Empty)
                return false;

            return await CanAccessSchoolAsync(userId, schoolId);
        }

        public void RemoveSchoolAccessCache(Guid userId, Guid schoolId)
        {
            if (userId != Guid.Empty && schoolId != Guid.Empty)
            {
                string cacheKey = $"school-access-{userId}-{schoolId}";
                _cache.Remove(cacheKey);
            }
        }
    }
}

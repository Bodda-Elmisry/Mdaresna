using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.ClassRoomManagement.Command
{
    public class ClassRoomCommandService : IBaseCommandService<ClassRoom>, IClassRoomCommandService
    {
        private readonly IBaseCommandRepository<ClassRoom> commandRepository;
        private readonly IBaseSharedRepository<ClassRoom> sharedRepository;
        private readonly AppDbContext context;

        public ClassRoomCommandService(IBaseCommandRepository<ClassRoom> commandRepository,
            IBaseSharedRepository<ClassRoom> sharedRepository,
            AppDbContext context)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
            this.context = context;
        }
        public bool Create(ClassRoom entity)
        {
            try
            {
                entity.Id = DataGenerationHelper.GenerateRowId();
                entity.CreateDate = DateTime.Now;
                entity.LastModifyDate = DateTime.Now;
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(ClassRoom entity)
        {
            try
            {
                entity = await sharedRepository.GetAsync(entity.Id);
                return commandRepository.Delete(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Update(ClassRoom entity)
        {
            try
            {
                var entry = context.Entry(entity);
                Guid? originalSupervisorId = null;

                if (entry.State == EntityState.Modified || entry.State == EntityState.Unchanged)
                {
                    originalSupervisorId = entry.Property(c => c.SupervisorId).OriginalValue;
                }
                else if (entry.State == EntityState.Detached)
                {
                    var originalClassRoom = context.ClassRooms.AsNoTracking().FirstOrDefault(c => c.Id == entity.Id);
                    if (originalClassRoom != null)
                    {
                        originalSupervisorId = originalClassRoom.SupervisorId;
                    }
                }

                entity.LastModifyDate = DateTime.Now;
                bool updated = commandRepository.Update(entity);

                if (updated && originalSupervisorId.HasValue && originalSupervisorId != entity.SupervisorId)
                {
                    var permissionsToRemove = context.userPermissionSchoolClassRooms
                        .Where(p => p.UserId == originalSupervisorId.Value && p.ClassRoomId == entity.Id)
                        .ToList();

                    if (permissionsToRemove.Any())
                    {
                        context.userPermissionSchoolClassRooms.RemoveRange(permissionsToRemove);
                        context.SaveChanges();
                    }
                }

                return updated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

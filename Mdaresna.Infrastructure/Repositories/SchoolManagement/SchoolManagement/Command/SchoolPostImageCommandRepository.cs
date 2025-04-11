using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Command;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Command
{
    public class SchoolPostImageCommandRepository : BaseCommandRepository<SchoolPostImage>, ISchoolPostImageCommandRepository
    {
        private readonly AppDbContext context;
        private readonly IBaseCommandBulkRepository<SchoolPostImage> baseCommandBulkRepository;

        public SchoolPostImageCommandRepository(AppDbContext context,
                                                IBaseCommandBulkRepository<SchoolPostImage> baseCommandBulkRepository) 
            : base(context)
        {
            this.context = context;
            this.baseCommandBulkRepository = baseCommandBulkRepository;
        }

        public async Task<bool> DeletePostImages(Guid postId)
        {
            try
            {
                var images = await context.SchoolPostImages.Where(i => i.PostId == postId).ToListAsync();

                var deleted = await baseCommandBulkRepository.DeleteBulk(images);

                return deleted;

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

    }
}

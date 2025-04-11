using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Command
{
    public class SchoolPostCommandService : IBaseCommandService<SchoolPost>, ISchoolPostCommandService
    {
        private readonly IBaseCommandRepository<SchoolPost> commandRepository;
        private readonly IBaseSharedRepository<SchoolPost> sharedRepository;
        private readonly ISchoolPostImageCommandRepository schoolPostImageCommandRepository;
        private readonly IBaseCommandBulkRepository<SchoolPostImage> imagesCommandBulkRepository;

        public SchoolPostCommandService(IBaseCommandRepository<SchoolPost> commandRepository,
            IBaseSharedRepository<SchoolPost> sharedRepository,
            IBaseCommandBulkRepository<SchoolPostImage> imagesCommandBulkRepository,
            ISchoolPostImageCommandRepository schoolPostImageCommandRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
            this.imagesCommandBulkRepository = imagesCommandBulkRepository;
            this.schoolPostImageCommandRepository = schoolPostImageCommandRepository;
        }
        public bool Create(SchoolPost entity)
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

        public async Task<bool> CreateAsync(SchoolPost entity, IEnumerable<string>? images)
        {
            try
            {
                entity.Id = DataGenerationHelper.GenerateRowId();
                entity.CreateDate = DateTime.Now;
                entity.LastModifyDate = DateTime.Now;
                entity.PostDate = DateTime.Now;
                var postAdded = commandRepository.Create(entity);
                var imagesAdded = true;

                if (postAdded && images != null)
                {
                    var postImages = new List<SchoolPostImage>();
                    foreach (var image in images)
                    {
                        var postImage = new SchoolPostImage
                        {
                            Id = DataGenerationHelper.GenerateRowId(),
                            PostId = entity.Id,
                            ImageUrl = image,
                            CreateDate = DateTime.Now,
                            LastModifyDate = DateTime.Now
                        };
                        postImages.Add(postImage);
                    }

                    imagesAdded = await imagesCommandBulkRepository.CreateBulk(postImages);
                }

                return postAdded && imagesAdded;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(SchoolPost entity)
        {
            try
            {
                entity = await sharedRepository.GetAsync(entity.Id);
                var postDeleted = commandRepository.Delete(entity);
                //var imagesDeleted = true;
                //if (postDeleted)
                //{
                //    imagesDeleted = await schoolPostImageCommandRepository.DeletePostImages(entity.PosterId);
                //}
                //return postDeleted && imagesDeleted;

                return postDeleted;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Update(SchoolPost entity)
        {
            try
            {
                entity.LastModifyDate = DateTime.Now;
                return commandRepository.Update(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

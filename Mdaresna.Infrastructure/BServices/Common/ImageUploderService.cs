using Mdaresna.Repository.IBServices.Common;
using Mdaresna.Repository.IRepositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.BServices.Common
{
    public class ImageUploderService : IImageUploderService
    {
        private readonly IImageUploderRepository imageUploderRepository;

        public ImageUploderService(IImageUploderRepository imageUploderRepository)
        {
            this.imageUploderRepository = imageUploderRepository;
        }

        public async Task<bool> UploadImage(Guid UserId, string filePath, bool isStudent)
        {
            return await imageUploderRepository.UploadImage(UserId, filePath, isStudent);
        }




    }
}

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

        public async Task<bool> UploadImage(Guid UserId, string filePath, int type)
        {
            return await imageUploderRepository.UploadImage(UserId, filePath, type);
        }

        public IEnumerable<byte> GetImageBytes(string imagePath)
        {
            List<byte> imageBytes = new List<byte>();

            using(FileStream fs = File.Open(imagePath, FileMode.Open))
            {
                var bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                fs.Close();
                imageBytes = bytes.ToList();
            }

            return imageBytes;
        }




    }
}

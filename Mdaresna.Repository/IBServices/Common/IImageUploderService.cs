using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IBServices.Common
{
    public interface IImageUploderService
    {
        Task<bool> UploadImage(Guid UserId, string filePath, bool isStudent);
    }
}

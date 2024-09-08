using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.Common
{
    public interface IImageUploderRepository
    {
        Task<bool> UploadImage(Guid UserId, string filePath, int type);
    }
}

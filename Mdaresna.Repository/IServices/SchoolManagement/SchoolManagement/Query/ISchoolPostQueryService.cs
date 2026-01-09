using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query
{
    public interface ISchoolPostQueryService : IBaseQueryService<SchoolPost>
    {
        Task<IEnumerable<PostResultDTO>> GetSchoolPostesWithImagesAsync(Guid schoolId, string searchText, int pageNumber);
        Task<PostResultDTO> GetPostWithImagesAsync(Guid postId);
        Task<IEnumerable<SchoolPostReportsCountResultDTO>> GetPostsWithReportsCountAsync(string? schoolName, int? minReportsCount, int? maxReportsCount, int pageNumber);
    }
}

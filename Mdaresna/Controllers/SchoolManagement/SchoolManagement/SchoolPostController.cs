using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Helpers;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
using Mdaresna.Infrastructure.BServices.Common;
using Mdaresna.Repository.IBServices.Common;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("SchoolPost")]
    public class SchoolPostController : Controller
    {
        private readonly ISchoolPostCommandService schoolPostCommandService;
        private readonly ISchoolPostQueryService schoolPostQueryService;
        private readonly IImageUploderService imageUploderService;
        private readonly AppSettingDTO appSettings;

        public SchoolPostController(ISchoolPostCommandService schoolPostCommandService,
                                    ISchoolPostQueryService schoolPostQueryService,
                                    IImageUploderService imageUploderService,
                                    IOptions<AppSettingDTO> appSettings)
        {
            this.schoolPostCommandService = schoolPostCommandService;
            this.schoolPostQueryService = schoolPostQueryService;
            this.imageUploderService = imageUploderService;
            this.appSettings = appSettings.Value;
        }

        [HttpPost("AddPost")]
        public async Task<IActionResult> AddPost([FromForm] AddSchoolPostDTO post)
        {
            if (post == null)
            {
                return BadRequest("Post cannot be null");
            }
            var schoolPost = new SchoolPost
            {
                Content = post.Content,
                PosterId = post.PosterId,
                SchoolId = post.SchoolId
            };
            var postImages = new List<string>();
            if(post.Images != null)
            {
                foreach (var file in post.Images)
                {
                    var imagePath = UploadImage(file);
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        postImages.Add(imagePath);
                    }
                }
            }

            var result = await schoolPostCommandService.CreateAsync(schoolPost, postImages);
            if (result)
            {
                return Ok("Post created successfully");
            }
            else
            {
                return StatusCode(500, "Internal server error");
            }
        }

        private string UploadImage(IFormFile file)
        {
            try
            {

                var fileParts = file.FileName.Split('.');
                string ext = fileParts[fileParts.Length - 1];

                var imageId = Guid.NewGuid();
                var localPath = Directory.GetCurrentDirectory();
                var directoryPathWithoutLocal = GetPathWithoutLocal(localPath);
                var fileName = string.Format("PI_{0}.{1}", imageId, ext);

                var filePath = Path.Combine(localPath,
                                            directoryPathWithoutLocal,
                                            fileName);

                if (!Directory.Exists(Path.Combine(localPath, directoryPathWithoutLocal)))
                {
                    Directory.CreateDirectory(Path.Combine(localPath, directoryPathWithoutLocal));
                }


                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                return Path.Combine(directoryPathWithoutLocal, fileName);
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        private string GetPathWithoutLocal(string localPath)
        {

            var directoryPath = string.Empty;
            directoryPath = Path.Combine(appSettings.ImagesPath,
                                        "SchoolPostsImages"
                                        );


            return directoryPath;
        }

        [HttpPost("GetPostsList")]
        public async Task<IActionResult> GetPostsList([FromBody] GetSchoolPostsListDTO dTO)
        {
            if (dTO.SchoolId == Guid.Empty)
            {
                return BadRequest("School ID cannot be empty");
            }

            var post = await schoolPostQueryService.GetSchoolPostesWithImagesAsync(dTO.SchoolId, dTO.SerachText, dTO.PageNumber);
            if (post == null)
            {
                return NotFound("Post not found");
            }

            return Ok(post);
        }

        [HttpPost("GetPost")]
        public async Task<IActionResult> GetPostsList([FromBody] SchoolPostIdDTO dTO)
        {
            if (dTO.PostId == Guid.Empty)
            {
                return BadRequest("Post ID cannot be empty");
            }

            var post = await schoolPostQueryService.GetPostWithImagesAsync(dTO.PostId);
            if (post == null)
            {
                return NotFound("Post not found");
            }

            return Ok(post);
        }

        [HttpDelete("DeletePost")]
        public async Task<IActionResult> DeletePost([FromBody] SchoolPostIdDTO dTO)
        {
            if (dTO.PostId == Guid.Empty)
            {
                return BadRequest("Post ID cannot be empty");
            }
            var post = await schoolPostQueryService.GetByIdAsync(dTO.PostId);

            var result = await schoolPostCommandService.DeleteAsync(post);
            if (result)
            {
                return Ok("Post deleted successfully");
            }
            else
            {
                return StatusCode(500, "Internal server error");
            }


        }
    }
}

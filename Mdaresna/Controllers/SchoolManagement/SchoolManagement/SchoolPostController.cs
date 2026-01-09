using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
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
        private readonly ISchoolPostReportCommandService schoolPostReportCommandService;
        private readonly ISchoolPostReportQueryService schoolPostReportQueryService;
        private readonly IImageUploderService imageUploderService;
        private readonly AppSettingDTO appSettings;

        public SchoolPostController(ISchoolPostCommandService schoolPostCommandService,
                                    ISchoolPostQueryService schoolPostQueryService,
                                    ISchoolPostReportCommandService schoolPostReportCommandService,
                                    ISchoolPostReportQueryService schoolPostReportQueryService,
                                    IImageUploderService imageUploderService,
                                    IOptions<AppSettingDTO> appSettings)
        {
            this.schoolPostCommandService = schoolPostCommandService;
            this.schoolPostQueryService = schoolPostQueryService;
            this.schoolPostReportCommandService = schoolPostReportCommandService;
            this.schoolPostReportQueryService = schoolPostReportQueryService;
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
            LogAddPost(post, postImages, result);
            if (result)
            {
                return Ok("Post created successfully");
            }
            else
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("ReportPost")]
        public IActionResult ReportPost([FromBody] AddSchoolPostReportDTO report)
        {
            if (report == null)
            {
                return BadRequest("Report cannot be null");
            }

            if (report.PostId == Guid.Empty || report.UserId == Guid.Empty)
            {
                return BadRequest("User ID and Post ID cannot be empty");
            }

            var postReport = new SchoolPostReport
            {
                PostId = report.PostId,
                UserId = report.UserId,
                Description = report.Description
            };

            var result = schoolPostReportCommandService.Create(postReport);
            if (result)
            {
                return Ok("Report created successfully");
            }

            return StatusCode(500, "Internal server error");
        }

        private void LogAddPost(AddSchoolPostDTO post, List<string> postImages, bool result)
        {
            try
            {
                var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "SchoolPosts");
                Directory.CreateDirectory(logDirectory);

                var logFilePath = Path.Combine(logDirectory, $"school-posts-{DateTime.UtcNow:yyyyMMdd}.log");
                var incomingImagesCount = post.Images?.Count() ?? 0;
                var contentPreview = string.IsNullOrWhiteSpace(post.Content)
                                        ? string.Empty
                                        : post.Content.Length <= 200 ? post.Content : post.Content[..200];

                var logEntry = $"[{DateTime.UtcNow:O}] SchoolId:{post.SchoolId}, PosterId:{post.PosterId}, IncomingImages:{incomingImagesCount}, SavedImages:{postImages.Count}, Result:{(result ? "Success" : "Failure")}, Content:\"{contentPreview}\"";

                System.IO.File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
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
            var deleted = await schoolPostReportCommandService.DeletePostReportsByPostIdAsync(dTO.PostId);

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

        [HttpDelete("DeletePostReports")]
        public async Task<IActionResult> DeletePostReports([FromBody] SchoolPostIdDTO dTO)
        {
            if (dTO.PostId == Guid.Empty)
            {
                return BadRequest("Post ID cannot be empty");
            }

            var deleted = await schoolPostReportCommandService.DeletePostReportsByPostIdAsync(dTO.PostId);
            if (deleted)
            {
                return Ok("Post reports deleted successfully");
            }

            return StatusCode(500, "Internal server error");
        }

        [HttpPost("GetPostsWithReportsCount")]
        public async Task<IActionResult> GetPostsWithReportsCount([FromBody] SchoolPostReportsFilterDTO filter)
        {
            if (filter == null)
            {
                return BadRequest("Filter cannot be null");
            }

            if (filter.MinReportsCount.HasValue && filter.MaxReportsCount.HasValue && filter.MinReportsCount > filter.MaxReportsCount)
            {
                return BadRequest("Min reports count cannot be greater than max reports count");
            }

            var pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            var posts = await schoolPostQueryService.GetPostsWithReportsCountAsync(filter.SchoolName, filter.MinReportsCount, filter.MaxReportsCount, pageNumber);
            return Ok(posts);
        }

        [HttpPost("GetReportsList")]
        public async Task<IActionResult> GetReportsList([FromBody] SchoolIdDTO dTO)
        {
            if (dTO.SchoolId == Guid.Empty)
            {
                return BadRequest("School ID cannot be empty");
            }

            var reports = await schoolPostReportQueryService.GetSchoolPostReportsAsync(dTO.SchoolId);
            return Ok(reports);
        }
    }
}

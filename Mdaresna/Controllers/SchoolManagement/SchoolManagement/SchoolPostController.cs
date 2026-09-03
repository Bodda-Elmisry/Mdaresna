using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Helpers;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
using Mdaresna.Repository.IBServices.Common;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mdaresna.Helpers;
using Microsoft.AspNetCore.Authorization;

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
        private readonly ISchoolAccessValidator schoolAccessValidator;
        private readonly AppSettingDTO appSettings;

        public SchoolPostController(
            ISchoolPostCommandService schoolPostCommandService,
            ISchoolPostQueryService schoolPostQueryService,
            ISchoolPostReportCommandService schoolPostReportCommandService,
            ISchoolPostReportQueryService schoolPostReportQueryService,
            IImageUploderService imageUploderService,
            ISchoolAccessValidator schoolAccessValidator,
            IOptions<AppSettingDTO> appSettings)
        {
            this.schoolPostCommandService = schoolPostCommandService;
            this.schoolPostQueryService = schoolPostQueryService;
            this.schoolPostReportCommandService = schoolPostReportCommandService;
            this.schoolPostReportQueryService = schoolPostReportQueryService;
            this.imageUploderService = imageUploderService;
            this.schoolAccessValidator = schoolAccessValidator;
            this.appSettings = appSettings.Value;
        }

        private Guid CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub) 
                                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
            }
        }

        private bool HasPermission(string permissionKey)
        {
            return User.FindAll("permissions").Any(claim =>
                string.Equals(
                    claim.Value.Split(':', 2)[0],
                    permissionKey,
                    StringComparison.OrdinalIgnoreCase));
        }

        [HttpPost("AddPost")]
        [Authorize]
        public async Task<IActionResult> AddPost([FromForm] AddSchoolPostDTO post)
        {
            if (post == null)
            {
                return BadRequest("Post cannot be null");
            }

            if (!Enum.IsDefined(typeof(SchoolPostVisibilityEnum), post.Visibility))
            {
                return BadRequest("Invalid post visibility");
            }

            post.PosterId = CurrentUserId;

            if (post.Visibility == SchoolPostVisibilityEnum.SchoolMembers &&
                !await schoolAccessValidator.CanAccessSchoolAsync(post.PosterId, post.SchoolId))
            {
                return Forbid();
            }

            var moderationDecision = SchoolPostModerationHelper.Evaluate(
                post.Content,
                post.Images?.Any() == true);

            if (!moderationDecision.AllowSubmission)
            {
                return Ok(new AddSchoolPostResultDTO
                {
                    Success = false,
                    Status = moderationDecision.Status.ToString(),
                    Message = moderationDecision.Message
                });
            }

            var schoolPost = new SchoolPost
            {
                Content = post.Content,
                PosterId = post.PosterId,
                SchoolId = post.SchoolId,
                Visibility = post.Visibility,
                ModerationStatus = moderationDecision.Status,
                ModerationReason = moderationDecision.ReasonCode
            };

            var postImages = new List<string>();
            if (post.Images != null)
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
                return Ok(new AddSchoolPostResultDTO
                {
                    Success = true,
                    Status = moderationDecision.Status.ToString(),
                    Message = moderationDecision.Message
                });
            }

            return StatusCode(500, "Internal server error");
        }

        [HttpPost("ReportPost")]
        [Authorize]
        public IActionResult ReportPost([FromBody] AddSchoolPostReportDTO report)
        {
            if (report == null)
            {
                return BadRequest("Report cannot be null");
            }

            report.UserId = CurrentUserId;

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
                if (!FileValidationHelper.ValidateImage(file, out _))
                {
                    return string.Empty;
                }

                string ext = Path.GetExtension(file.FileName).TrimStart('.');

                var imageId = Guid.NewGuid();
                var localPath = Directory.GetCurrentDirectory();
                var directoryPathWithoutLocal = GetPathWithoutLocal(localPath);
                var fileName = string.Format("PI_{0}.{1}", imageId, ext);

                var filePath = Path.Combine(
                    localPath,
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
            catch
            {
                return string.Empty;
            }
        }

        private string GetPathWithoutLocal(string localPath)
        {
            var directoryPath = string.Empty;
            directoryPath = Path.Combine(
                appSettings.ImagesPath,
                "SchoolPostsImages");

            return directoryPath;
        }

        [HttpPost("GetPostsList")]
        public async Task<IActionResult> GetPostsList([FromBody] GetSchoolPostsListDTO dTO)
        {
            if (dTO.SchoolId == Guid.Empty)
            {
                return BadRequest("School ID cannot be empty");
            }

            var viewerUserId = CurrentUserId == Guid.Empty ? (Guid?)null : CurrentUserId;
            var includeSchoolMembers = viewerUserId.HasValue &&
                await schoolAccessValidator.CanAccessSchoolAsync(viewerUserId.Value, dTO.SchoolId);

            var post = await schoolPostQueryService.GetSchoolPostesWithImagesAsync(
                dTO.SchoolId,
                viewerUserId,
                includeSchoolMembers,
                dTO.SerachText,
                dTO.PageNumber);
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

            var schoolPost = await schoolPostQueryService.GetByIdAsync(dTO.PostId);
            if (schoolPost == null || schoolPost.Deleted)
            {
                return NotFound("Post not found");
            }

            var includeSchoolMembers = CurrentUserId != Guid.Empty &&
                await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, schoolPost.SchoolId);
            var post = await schoolPostQueryService.GetPostWithImagesAsync(dTO.PostId, includeSchoolMembers);
            if (post == null)
            {
                return NotFound("Post not found");
            }

            return Ok(post);
        }

        [HttpDelete("DeletePost")]
        [Authorize]
        public async Task<IActionResult> DeletePost([FromBody] SchoolPostIdDTO dTO)
        {
            if (dTO.PostId == Guid.Empty)
            {
                return BadRequest("Post ID cannot be empty");
            }

            await schoolPostReportCommandService.DeletePostReportsByPostIdAsync(dTO.PostId);
            var post = await schoolPostQueryService.GetByIdAsync(dTO.PostId);
            var result = await schoolPostCommandService.DeleteAsync(post);
            if (result)
            {
                return Ok("Post deleted successfully");
            }

            return StatusCode(500, "Internal server error");
        }

        [HttpPost("ApprovePost")]
        [Authorize]
        public async Task<IActionResult> ApprovePost([FromBody] SchoolPostIdDTO dTO)
        {
            if (dTO.PostId == Guid.Empty)
            {
                return BadRequest("Post ID cannot be empty");
            }

            var post = await schoolPostQueryService.GetByIdAsync(dTO.PostId);
            if (post == null || post.Deleted)
            {
                return NotFound("Post not found");
            }

            post.ModerationStatus = SchoolPostModerationStatusEnum.Approved;
            post.ModerationReason = null;

            var result = schoolPostCommandService.Update(post);
            if (result)
            {
                return Ok("Post approved successfully");
            }

            return StatusCode(500, "Internal server error");
        }

        [HttpDelete("DeletePostReports")]
        [Authorize]
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
        [Authorize]
        public async Task<IActionResult> GetPostsWithReportsCount([FromBody] SchoolPostReportsFilterDTO filter)
        {
            if (filter == null)
            {
                return BadRequest("Filter cannot be null");
            }

            var canViewAllReportedPosts = HasPermission("ShowReportedPosts");
            var canViewSchoolReportedPosts = HasPermission("ShowSchoolReportedPosts");
            if (!canViewAllReportedPosts && !canViewSchoolReportedPosts)
            {
                return Forbid();
            }

            if (!canViewAllReportedPosts)
            {
                if (!filter.SchoolId.HasValue || filter.SchoolId.Value == Guid.Empty)
                {
                    return BadRequest("School ID is required");
                }

                if (!await schoolAccessValidator.CanAccessSchoolAsync(
                        CurrentUserId,
                        filter.SchoolId.Value))
                {
                    return Forbid();
                }
            }

            if (filter.MinReportsCount.HasValue &&
                filter.MaxReportsCount.HasValue &&
                filter.MinReportsCount > filter.MaxReportsCount)
            {
                return BadRequest("Min reports count cannot be greater than max reports count");
            }

            var pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            var posts = await schoolPostQueryService.GetPostsWithReportsCountAsync(
                filter.SchoolId,
                filter.SchoolName,
                filter.MinReportsCount,
                filter.MaxReportsCount,
                pageNumber);
            return Ok(posts);
        }

        [HttpPost("GetReportsList")]
        [Authorize]
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

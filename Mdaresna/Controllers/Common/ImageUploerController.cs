using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.Helpers;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.Repository.IBServices.Common;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Helpers;
using Mdaresna.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

namespace Mdaresna.Controllers.Common
{
    [Route("ImageUploder")]
    [Authorize]
    public class ImageUploerController : Controller
    {
        private readonly IImageUploderService imageUploderService;
        private readonly ISchoolCommandService schoolCommandService;
        private readonly ISchoolQueryService schoolQueryService;
        private readonly ISchoolAccessValidator schoolAccessValidator;
        private readonly AppSettingDTO _appSettings;
        private const long MaxSchoolLogoSizeInBytes = 2 * 1024 * 1024;
        private static readonly string[] AllowedSchoolLogoExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public ImageUploerController(IImageUploderService imageUploderService,
                                     ISchoolCommandService schoolCommandService,
                                     ISchoolQueryService schoolQueryService,
                                     ISchoolAccessValidator schoolAccessValidator,
                                     IOptions<AppSettingDTO> appSettings)
        {
            this.imageUploderService = imageUploderService;
            this.schoolCommandService = schoolCommandService;
            this.schoolQueryService = schoolQueryService;
            this.schoolAccessValidator = schoolAccessValidator;
            this._appSettings = appSettings.Value;
        }

        private Guid CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
                    ? userId
                    : Guid.Empty;
            }
        }

        [HttpPost("UploadSchoolLogo")]
        [PermissionAuthorize("UploadeSchoolImage")]
        public async Task<IActionResult> UploadSchoolLogo([FromForm] UploadSchoolLogoDTO dto)
        {
            if (dto == null || dto.SchoolId == Guid.Empty)
            {
                return BadRequest("School ID cannot be empty");
            }

            if (!await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, dto.SchoolId))
            {
                return Forbid();
            }

            if (!FileValidationHelper.ValidateImage(dto.File, out var validationError))
            {
                return BadRequest(validationError);
            }

            if (dto.File.Length > MaxSchoolLogoSizeInBytes)
            {
                return BadRequest("School logo size cannot exceed 2 MB.");
            }

            var extension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
            if (!AllowedSchoolLogoExtensions.Contains(extension))
            {
                return BadRequest("School logo must be a JPG, PNG, or WebP image.");
            }

            var school = await schoolQueryService.GetByIdAsync(dto.SchoolId);
            if (school == null || school.Deleted)
            {
                return NotFound("School not found");
            }

            var relativeDirectory = Path.Combine(
                _appSettings.ImagesPath,
                "Schools",
                dto.SchoolId.ToString(),
                "Logo");
            var absoluteDirectory = Path.Combine(Directory.GetCurrentDirectory(), relativeDirectory);
            Directory.CreateDirectory(absoluteDirectory);

            var fileName = $"logo_{Guid.NewGuid():N}{extension}";
            var relativePath = Path.Combine(relativeDirectory, fileName);
            var absolutePath = Path.Combine(absoluteDirectory, fileName);

            await using (var stream = new FileStream(absolutePath, FileMode.CreateNew))
            {
                await dto.File.CopyToAsync(stream);
            }

            var oldLogoPath = school.LogoUrl;
            try
            {
                school.LogoUrl = relativePath;
                if (!schoolCommandService.Update(school))
                {
                    DeleteLogoFile(relativePath, dto.SchoolId);
                    return BadRequest("Error in uploading school logo");
                }
            }
            catch
            {
                DeleteLogoFile(relativePath, dto.SchoolId);
                throw;
            }

            DeleteLogoFile(oldLogoPath, dto.SchoolId);
            return Ok(BuildPublicUrl(relativePath));
        }

        [HttpDelete("RemoveSchoolLogo")]
        [PermissionAuthorize("UploadeSchoolImage")]
        public async Task<IActionResult> RemoveSchoolLogo([FromBody] SchoolIdDTO dto)
        {
            if (dto == null || dto.SchoolId == Guid.Empty)
            {
                return BadRequest("School ID cannot be empty");
            }

            if (!await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, dto.SchoolId))
            {
                return Forbid();
            }

            var school = await schoolQueryService.GetByIdAsync(dto.SchoolId);
            if (school == null || school.Deleted)
            {
                return NotFound("School not found");
            }

            var oldLogoPath = school.LogoUrl;
            if (string.IsNullOrWhiteSpace(oldLogoPath))
            {
                return Ok("School logo removed");
            }

            school.LogoUrl = null;
            if (!schoolCommandService.Update(school))
            {
                return BadRequest("Error in removing school logo");
            }

            DeleteLogoFile(oldLogoPath, dto.SchoolId);
            return Ok("School logo removed");
        }

        [HttpPost("UploadImage")]
        public async Task<IActionResult> UploadImage(UploadImageDTO uploadImageDTO)
        {
            try
            {
                if (!FileValidationHelper.ValidateImage(uploadImageDTO.File, out var validationError))
                {
                    return BadRequest(validationError);
                }

                string ext = Path.GetExtension(uploadImageDTO.File.FileName).TrimStart('.');

                var localPath = Directory.GetCurrentDirectory();
                var directoryPathWithoutLocal = GetPathWithoutLocal(localPath, uploadImageDTO);
                var fileName = string.Format("PI_{0}.{1}", Guid.NewGuid().ToString(), ext); //uploadImageDTO.UserId.ToString() + "." + ext;

                var filePath = Path.Combine(localPath,
                                            directoryPathWithoutLocal,
                                            fileName);

                if (!Directory.Exists(Path.Combine(localPath,directoryPathWithoutLocal)))
                {
                    Directory.CreateDirectory(Path.Combine(localPath, directoryPathWithoutLocal));
                }

                //filePath += string.Format("\\PI_{0}.{1}", uploadImageDTO.UserId.ToString(), ext);

                //delete personal images (user, student or school)
                if (System.IO.File.Exists(filePath) && uploadImageDTO.Type <= 3 && uploadImageDTO.Type > 0)
                    System.IO.File.Delete(filePath);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadImageDTO.File.CopyTo(stream);
                }

                var uploader = await imageUploderService.UploadImage(uploadImageDTO.UserId,
                                                                 Path.Combine(directoryPathWithoutLocal,fileName),
                                                                 uploadImageDTO.Type);
                if (uploader)
                    return Ok(Path.Combine(SettingsHelper.GetAppUrl(), directoryPathWithoutLocal, fileName));
                return BadRequest("Error in uploade image");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UploadImagesBulk")]
        public async Task<IActionResult> UploadImagesBulk([FromForm] UploadImageBulkDTO dtos)
        {
            try
            {
                foreach(var model in dtos.Files)
                {
                    var dto = new UploadImageDTO
                    {
                        File = model,
                        Type = dtos.Type,
                        UserId = dtos.UserId
                    };

                    await UploadImage(dto);
                }
                
                    return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GetPathWithoutLocal(string localPath ,UploadImageDTO uploadImageDTO)
        {

            /*
            1- Student Image
            2- User Image
            3- School Profile Image
            5- School Conatact Type Icone
            6- Classroom WCS
             */

            var directoryPath = string.Empty;
            switch (uploadImageDTO.Type)
            {
                case 1:
                case 2:
                    directoryPath = Path.Combine(_appSettings.ImagesPath,
                                        uploadImageDTO.UserId.ToString(),
                                        "PersonalImage"
                                        );
                    break;
                case 3:
                    directoryPath = Path.Combine(_appSettings.ImagesPath,
                                        "Schools",
                                        uploadImageDTO.UserId.ToString(),
                                        "ProfileImage"
                                        );
                    break;
                case 5:
                    directoryPath = Path.Combine(_appSettings.ImagesPath,
                                        "SchoolContactTypes",
                                        uploadImageDTO.UserId.ToString()
                                        );
                    break;
                case 6:
                    directoryPath = Path.Combine(_appSettings.ImagesPath,
                                        "ClassroomsWCS",
                                        uploadImageDTO.UserId.ToString()
                                        );
                    break;
            }
             

            return directoryPath;
        }

        private string BuildPublicUrl(string relativePath)
        {
            return $"{SettingsHelper.GetAppUrl().TrimEnd('/')}/{relativePath.Replace("\\", "/").TrimStart('/')}";
        }

        private void DeleteLogoFile(string? storedPath, Guid schoolId)
        {
            if (string.IsNullOrWhiteSpace(storedPath))
            {
                return;
            }

            var fileName = Path.GetFileName(storedPath.Replace('/', Path.DirectorySeparatorChar));
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            var logoDirectory = Path.Combine(
                Directory.GetCurrentDirectory(),
                _appSettings.ImagesPath,
                "Schools",
                schoolId.ToString(),
                "Logo");
            var filePath = Path.Combine(logoDirectory, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }


    }
}

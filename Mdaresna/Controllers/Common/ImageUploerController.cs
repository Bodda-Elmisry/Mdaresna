using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.Repository.IBServices.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Mdaresna.Controllers.Common
{
    [Route("ImageUploder")]
    public class ImageUploerController : Controller
    {
        private readonly IImageUploderService imageUploderService;
        private readonly AppSettingDTO _appSettings;

        public ImageUploerController(IImageUploderService imageUploderService,
                                     IOptions<AppSettingDTO> appSettings)
        {
            this.imageUploderService = imageUploderService;
            this._appSettings = appSettings.Value;
        }

        [HttpPost("UploadImage")]
        public async Task<IActionResult> UploadImage(UploadImageDTO uploadImageDTO)
        {
            try
            {

                if (uploadImageDTO.File == null || uploadImageDTO.File.Length == 0)
                {
                    throw new ArgumentException("No file provided or file is empty.");
                }

                var fileParts = uploadImageDTO.File.FileName.Split('.');
                string ext = fileParts[fileParts.Length - 1];

                var localPath = Directory.GetCurrentDirectory();
                var filePath = Path.Combine(localPath,
                                            _appSettings.ImagesPath,
                                            uploadImageDTO.UserId.ToString(),
                                            "PersonalImage",
                                            string.Format("PI_{0}.{1}",
                                                          uploadImageDTO.UserId.ToString(),
                                                          ext)
                                            );

                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                System.IO.File.Delete(filePath);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    uploadImageDTO.File.CopyTo(stream);
                }

                var uploader = await imageUploderService.UploadImage(uploadImageDTO.UserId,
                                                                 filePath,
                                                                 uploadImageDTO.IsStudent);
                if (uploader)
                    return Ok("File Uploade Correct");
                return BadRequest("Error in uploade image");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.Helpers;
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
                var directoryPathWithoutLocal = GetPathWithoutLocal(localPath, uploadImageDTO);
                var fileName = string.Format("PI_{0}.{1}", uploadImageDTO.UserId.ToString(), ext); //uploadImageDTO.UserId.ToString() + "." + ext;

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


    }
}

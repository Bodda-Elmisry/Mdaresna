using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("api/School")]
    public class SchoolController : Controller
    {
        private readonly ISchoolCommandService schoolCommandService;
        private readonly ISchoolQueryService schoolQueryService;
        private readonly IClassRoomCommandService classRoomCommandService;
        private readonly IClassRoomQueryService classRoomQueryService;
        private readonly IStudentQueryService studentQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IUserDeviceQueryService userDeviceQueryService;
        private readonly IUserRoleQueryService userRoleQueryService;
        private readonly IUserPermissionQueryService userPermissionQueryService;

        public SchoolController(ISchoolCommandService schoolCommandService,
                                ISchoolQueryService schoolQueryService,
                                IClassRoomCommandService classRoomCommandService,
                                IClassRoomQueryService classRoomQueryService,
                                IStudentQueryService studentQueryService,
                                INotificationFactory notificationFactory,
                                IUserDeviceQueryService userDeviceQueryService,
                                IUserRoleQueryService userRoleQueryService,
                                IUserPermissionQueryService userPermissionQueryService)
        {
            this.schoolCommandService = schoolCommandService;
            this.schoolQueryService = schoolQueryService;
            this.classRoomCommandService = classRoomCommandService;
            this.classRoomQueryService = classRoomQueryService;
            this.studentQueryService = studentQueryService;
            this.notificationFactory = notificationFactory;
            this.userDeviceQueryService = userDeviceQueryService;
            this.userRoleQueryService = userRoleQueryService;
            this.userPermissionQueryService = userPermissionQueryService;
        }

        [HttpPost("AddSchool")]
        public async Task<IActionResult> CreateNewSchool([FromBody] CreateSchoolDTO School)
        {
            try
            {
                var newSchool = new School
                {
                    Name = School.SchoolName,
                    About = School.About,
                    Active = false,
                    AvailableCoins = 0,
                    ImageUrl = string.Empty,
                    SchoolAdminId = School.SchoolAdminId,
                    SchoolTypeId = School.SchoolTypeId,
                    Vesion = School.Vesion
                };

                var added = schoolCommandService.Create(newSchool);
                if (added)
                {
                    var applicationAdmins = await userRoleQueryService.GetRoleUsersAsync(Guid.Parse("228AE7F5-C704-4660-AEB0-0E1F43112AE1"), null);
                    var managerIds = applicationAdmins.Select(a=> a.UserId).ToList();
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var devices = await userDeviceQueryService.GetUsersDevicesAsync(managerIds);
                    if (devices.Count() > 0)
                    {
                        var message = $"School '{newSchool.Name}' added";
                        var tokens = devices.Select(d => d.FcmToken).ToList();
                        await notificationProvider.SendToMultiUsersAsync(tokens, "New School", message);
                    }
                    return Ok(await schoolQueryService.GetSchoolById(newSchool.Id));
                }

                return BadRequest();

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("UpdateSchool")]
        public async Task<IActionResult> UpdateSchoolInfo([FromBody] UpdateSchoolDTO SchoolInfo)
        {
            try
            {
                var school = await schoolQueryService.GetByIdAsync(SchoolInfo.Id);

                if (school == null)
                    return BadRequest("There is no school to update");

                school.Name = SchoolInfo.SchoolName;
                school.About = SchoolInfo.About;
                school.Vesion = SchoolInfo.Vesion;
                school.SchoolAdminId = SchoolInfo.SchoolAdminId;

                var updated = schoolCommandService.Update(school);
                if (updated)
                    return Ok(await schoolQueryService.GetSchoolById(school.Id));

                return BadRequest("Error in update school");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("ChangeSchoolCoinType")]
        public async Task<IActionResult> ChangeSchoolCoinType([FromBody] ChangeSchoolCoinTypeDTO dto)
        {
            try
            {
                var school = await schoolQueryService.GetByIdAsync(dto.SchoolId);

                if (school == null)
                    return BadRequest("There is no school to update");

                school.CoinTypeId = dto.CoinTypeId;

                var updated = schoolCommandService.Update(school);
                if (updated)
                    return Ok(await schoolQueryService.GetSchoolById(school.Id));

                return BadRequest("Error in change school coin type");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("AddCoinsToSchool")]
        public async Task<IActionResult> AddCoinsToSchool([FromBody] AddCoinsToSchoolDTO coinsdto)
        {
            try
            {
                var school = await schoolQueryService.GetByIdAsync(coinsdto.SchoolId);

                if (school == null)
                    return BadRequest("There is no school to add coins");

                school.AvailableCoins = school.AvailableCoins + coinsdto.CoinsCount;

                var updated = schoolCommandService.Update(school);
                if (updated)
                {
                    var usersIds = await userPermissionQueryService.GetPermissionUsersIds(Guid.Parse("9DD22E15-9701-492B-AE20-985B8927F3BF"), school.Id);
                    var userIdsList = usersIds.ToList();
                    userIdsList.Add(school.SchoolAdminId);
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var devices = await userDeviceQueryService.GetUsersDevicesAsync(userIdsList);
                    if (devices.Count() > 0)
                    {
                        var message = $"Balance changed in school '{school.Name}'";
                        var tokens = devices.Select(d => d.FcmToken).ToList();
                        await notificationProvider.SendToMultiUsersAsync(tokens, "Balance Changed", message);
                    }
                    return Ok(await schoolQueryService.GetSchoolById(school.Id));
                }

                return BadRequest("Error in update school");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("GetAll")]
        public async Task<IActionResult> GetSchools([FromBody] GetSchoolDTO dTO)
        {
            try
            {
                var result = await schoolQueryService.GetSchoolsList(dTO.Name, dTO.Active, dTO.SchoolTypeId, dTO.CoinTypeId, dTO.SchoolAdminId, dTO.PageNumber, dTO.NewSchools);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ChangeActivation")]
        public async Task<IActionResult> ChangeSchoolActivition([FromBody] ChangeSchoolActivationDTO dTO)
        {
            try
            {
                var school = await schoolQueryService.GetByIdAsync(dTO.SchoolId);

                if (school == null)
                    return BadRequest("There is not school to update");
                if (school != null && school.Active == dTO.Active)
                    return Ok("Activation Changed");

                school.Active = dTO.Active;

                var updated = schoolCommandService.Update(school);
                if (updated)
                {
                    var schoolUsersIds = await schoolQueryService.GetSchoolUsersIds(school.Id);
                    var schoolUsersIdsList = schoolUsersIds.ToList();
                    schoolUsersIdsList.Add(school.SchoolAdminId);
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var devices = await userDeviceQueryService.GetUsersDevicesAsync(schoolUsersIdsList);
                    if (devices.Count() > 0)
                    {
                        var message = dTO.Active ? $"School '{school.Name}' activated" : $"School '{school.Name}' deactivated";
                        var tokens = devices.Select(d => d.FcmToken).ToList();
                        await notificationProvider.SendToMultiUsersAsync(tokens, "School Activation", message);
                    }
                    return Ok("Activation Changed");
                }

                return BadRequest("Error in Updating school");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ActivateNewSchool")]
        public async Task<IActionResult> ActivateNewSchool([FromBody] ActivateNewSchoolDTO dTO)
        {
            try
            {
                var school = await schoolQueryService.GetByIdAsync(dTO.SchoolId);

                if (school == null)
                    return BadRequest("There is not school to update");
                if (school != null && school.Active == true && school.CoinTypeId != null && school.CoinTypeId == dTO.CoinTypeId)
                    return Ok("School activated");

                school.Active = true;
                school.CoinTypeId = dTO.CoinTypeId;

                var updated = schoolCommandService.Update(school);

                return updated ? Ok("School activated") : BadRequest("Error in Updating school");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetById")]
        public async Task<IActionResult> GetSchoolById([FromBody] SchoolIdDTO schoolIdDTO)
        {
            try
            {
                var school = await schoolQueryService.GetSchoolById(schoolIdDTO.SchoolId);
                return Ok(school);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetUserSchools")]
        public async Task<IActionResult> GetUserSchools([FromBody] GetUserSchoolsDTO DTO)
        {
            try
            {
                var schools = await schoolQueryService.GetUserSchools(DTO.UserId, DTO.Active);
                return Ok(schools);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteSchool")]
        public async Task<IActionResult> SoftDeleteSchool([FromBody] SchoolIdDTO dto)
        {
            try
            {
                var schoolStudents = await studentQueryService.GetStudentsBySchoolIdViewAsync(dto.SchoolId, string.Empty, string.Empty);

                if (schoolStudents != null && schoolStudents.Count() > 0)
                    return BadRequest("There are students in this school, you can't delete it");

                var school = await schoolQueryService.GetByIdAsync(dto.SchoolId);

                if (school == null)
                    return BadRequest("There is no school to delete");

                var classRooms = await classRoomQueryService.GetCLassroomsBySchoolIdAsync(school.Id);

                foreach (var classRoom in classRooms)
                {
                    classRoom.Deleted = true;

                    classRoomCommandService.Update(classRoom);
                }

                school.Deleted = true;
                var deleted = schoolCommandService.Update(school);

                return deleted ? Ok("School deleted") : BadRequest("Error in deleting school");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("RemoveSchoolImage")]
        public async Task<IActionResult> RemoveSchoolImage([FromBody] ImagePathDTO dto)
        {
            try
            {
                var deleted = await schoolCommandService.DeleteSchoolImageByImageNameAsync(dto.ImagePath);

                return deleted ? Ok("School image removed") : BadRequest("Error in removing school image");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}

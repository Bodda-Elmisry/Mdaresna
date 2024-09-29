using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("api/School")]
    public class SchoolController : Controller
    {
        private readonly ISchoolCommandService schoolCommandService;
        private readonly ISchoolQueryService schoolQueryService;

        public SchoolController(ISchoolCommandService schoolCommandService, 
                                ISchoolQueryService schoolQueryService)
        {
            this.schoolCommandService = schoolCommandService;
            this.schoolQueryService = schoolQueryService;
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
                    return Ok(await schoolQueryService.GetSchoolById(newSchool.Id));

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
                    return Ok(await schoolQueryService.GetSchoolById(school.Id));

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
                var result = await schoolQueryService.GetSchoolsList(dTO.Name, dTO.Active, dTO.SchoolTypeId, dTO.CoinTypeId, dTO.SchoolAdminId, dTO.PageNumber);
                return Ok(result);
            }
            catch(Exception ex)
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

                return updated ? Ok("Activation Changed") : BadRequest("Error in Updating school");
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


    }
}

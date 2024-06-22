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
        public IActionResult CreateNewSchool([FromBody] CreateSchoolDTO School)
        {
            try
            {
                var newSchool = new School
                {
                    Name = School.SchoolName,
                    About = School.About,
                    Active = false,
                    AvailableCoins = 0,
                    ImageUrl = School.ImageUrl,
                    SchoolAdminId = School.SchoolAdminId,
                    SchoolTypeId = School.SchoolTypeId,
                    Vesion = School.Vesion
                };

                var added = schoolCommandService.Create(newSchool);
                if (added)
                    return Ok(newSchool);

                return BadRequest();

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetSchools()
        {
            try
            {
                var result = await schoolQueryService.GetAllAsync();
                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetById")]
        public async Task<IActionResult> GetSchoolById([FromBody] SchoolIdDTO schoolIdDTO)
        {
            try
            {
                var school = await schoolQueryService.GetByIdAsync(schoolIdDTO.SchoolId);
                return Ok(school);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}

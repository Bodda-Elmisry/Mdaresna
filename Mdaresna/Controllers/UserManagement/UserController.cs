using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.UserManagementDTO;
using Mdaresna.Repository.IServices.UserManagement.Command;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.UserManagement
{
    [Route("API/User")]
    public class UserController : Controller
    {
        private readonly IUserQueryService userQueryService;
        private readonly IUserCommandService userCommandService;

        public UserController(IUserQueryService userQueryService, IUserCommandService userCommandService)
        {
            this.userQueryService = userQueryService;
            this.userCommandService = userCommandService;
        }

        [HttpPost("CreateUser")]
        public IActionResult CreateUser(User user)
        {
            try
            {
                var add = userCommandService.Create(user);

                return add ? Ok(user) : BadRequest("Error In User");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDTO dTO)
        {
            try
            {
                var user = await userQueryService.GetByIdAsync(dTO.Id);

                if (user == null)
                {
                    return BadRequest("There is no user to update");
                }

                user.UserName = dTO.UserName;
                user.FirstName = dTO.FirstName;
                user.MiddelName = dTO.MiddelName;
                user.LastName = dTO.LastName;
                user.BirthDay = dTO.BirthDay;
                user.Address = dTO.Address;
                user.City = dTO.City;
                user.Region = dTO.Region;
                user.Contry = dTO.Country;
                user.Email = dTO.Email;

                var updated = userCommandService.Update(user);

                if (!updated)
                    return BadRequest("Error in updating user");

                return Ok(await userQueryService.GetUserById(dTO.Id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetByPhoneNumber")]
        public async Task<IActionResult> GetUserByPhoneNumber([FromBody] PhoneDTO phoneDTO)
        {
            try
            {
                var user = await userQueryService.GetUserByPhoneNumber(phoneDTO.PhoneNumber);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetUserById")]
        public async Task<IActionResult> GetUserById([FromBody] UserIdDTO dTO)
        {
            try
            {
                var user = await userQueryService.GetUserById(dTO.UserId);

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteUser")]
        public async Task<IActionResult> SoftDeleteUser([FromBody] UserIdDTO dTO)
        {
            try
            {
                var user = await userQueryService.GetByIdAsync(dTO.UserId);

                if (user == null)
                    return BadRequest("There is no user to delete");

                user.Deleted = true;

                var deleted = userCommandService.Update(user);

                return deleted ? Ok("User Deleted") : BadRequest("Error in delete user");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



    }
}

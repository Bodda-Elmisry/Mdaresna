using Mdaresna.Doamin.Models.UserManagement;
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
    }
}

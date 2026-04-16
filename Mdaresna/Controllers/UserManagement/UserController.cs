using Mdaresna.Doamin.DTOs.UserManagement;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.UserManagementDTO;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IServices.UserManagement.Command;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Controllers.UserManagement
{
    [Route("API/User")]
    public class UserController : Controller
    {
        private readonly IUserQueryService userQueryService;
        private readonly IUserCommandService userCommandService;
        private readonly IUserReportCommandService userReportCommandService;
        private readonly IUserReportQueryService userReportQueryService;
        private readonly AppDbContext context;

        public UserController(IUserQueryService userQueryService,
                              IUserCommandService userCommandService,
                              IUserReportCommandService userReportCommandService,
                              IUserReportQueryService userReportQueryService,
                              AppDbContext context)
        {
            this.userQueryService = userQueryService;
            this.userCommandService = userCommandService;
            this.userReportCommandService = userReportCommandService;
            this.userReportQueryService = userReportQueryService;
            this.context = context;
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

        [HttpPost("UpdateUserLanguage")]
        public async Task<IActionResult> UpdateUserLanguage([FromBody] UpdateUserLanguageDTO dTO)
        {
            try
            {
                var user = await userQueryService.GetByIdAsync(dTO.UserId);

                if (user == null)
                {
                    return BadRequest("There is no user to update");
                }

                user.Language = dTO.Language;

                var updated = userCommandService.Update(user);

                if (!updated)
                    return BadRequest("Error in updating user");

                return Ok("Language Updated");
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

        [HttpPost("ReportUser")]
        public async Task<IActionResult> ReportUser([FromBody] AddUserReportDTO report)
        {
            try
            {
                if (report == null)
                {
                    return BadRequest("Report cannot be null");
                }

                if (report.ReporterUserId == Guid.Empty || report.ReportedUserId == Guid.Empty)
                {
                    return BadRequest("Reporter user ID and reported user ID cannot be empty");
                }

                if (report.ReporterUserId == report.ReportedUserId)
                {
                    return BadRequest("Reporter user cannot report the same user");
                }

                if (string.IsNullOrWhiteSpace(report.Description))
                {
                    return BadRequest("Report description cannot be empty");
                }

                var reporterUser = await userQueryService.GetByIdAsync(report.ReporterUserId);
                if (reporterUser == null || reporterUser.Deleted)
                {
                    return BadRequest("Reporter user not found");
                }

                var reportedUser = await userQueryService.GetByIdAsync(report.ReportedUserId);
                if (reportedUser == null || reportedUser.Deleted)
                {
                    return BadRequest("Reported user not found");
                }

                var userReport = new UserReport
                {
                    ReporterUserId = report.ReporterUserId,
                    ReportedUserId = report.ReportedUserId,
                    Description = report.Description
                };

                var created = userReportCommandService.Create(userReport);
                return created ? Ok("Report created successfully") : BadRequest("Error in creating report");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("BlockUser")]
        public async Task<IActionResult> BlockUser([FromBody] AddUserBlockDTO block)
        {
            try
            {
                if (block == null)
                {
                    return BadRequest("Block request cannot be null");
                }

                if (block.BlockerUserId == Guid.Empty || block.BlockedUserId == Guid.Empty)
                {
                    return BadRequest("Blocker user ID and blocked user ID cannot be empty");
                }

                if (block.BlockerUserId == block.BlockedUserId)
                {
                    return BadRequest("User cannot block the same user");
                }

                var blockerUser = await userQueryService.GetByIdAsync(block.BlockerUserId);
                if (blockerUser == null || blockerUser.Deleted)
                {
                    return BadRequest("Blocker user not found");
                }

                var blockedUser = await userQueryService.GetByIdAsync(block.BlockedUserId);
                if (blockedUser == null || blockedUser.Deleted)
                {
                    return BadRequest("Blocked user not found");
                }

                var existingBlock = await context.UserBlocks.FirstOrDefaultAsync(x =>
                    x.BlockerUserId == block.BlockerUserId &&
                    x.BlockedUserId == block.BlockedUserId);

                if (existingBlock != null)
                {
                    if (existingBlock.Deleted)
                    {
                        existingBlock.Deleted = false;
                        existingBlock.LastModifyDate = DateTime.Now;
                        context.UserBlocks.Update(existingBlock);
                        await context.SaveChangesAsync();
                    }

                    return Ok("User blocked successfully");
                }

                context.UserBlocks.Add(new UserBlock
                {
                    Id = DataGenerationHelper.GenerateRowId(),
                    BlockerUserId = block.BlockerUserId,
                    BlockedUserId = block.BlockedUserId,
                    CreateDate = DateTime.Now,
                    LastModifyDate = DateTime.Now
                });

                await context.SaveChangesAsync();
                return Ok("User blocked successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteUserReports")]
        public async Task<IActionResult> DeleteUserReports([FromBody] UserIdDTO dTO)
        {
            try
            {
                if (dTO.UserId == Guid.Empty)
                {
                    return BadRequest("User ID cannot be empty");
                }

                var deleted = await userReportCommandService.DeleteUserReportsByReportedUserIdAsync(dTO.UserId);
                return deleted ? Ok("User reports deleted") : BadRequest("Error in deleting user reports");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetUsersWithReportsCount")]
        public async Task<IActionResult> GetUsersWithReportsCount([FromBody] UserReportsFilterDTO filter)
        {
            try
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
                var users = await userReportQueryService.GetUsersWithReportsCountAsync(filter.UserName, filter.MinReportsCount, filter.MaxReportsCount, pageNumber);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetUserReportsList")]
        public async Task<IActionResult> GetUserReportsList([FromBody] UserIdDTO dTO)
        {
            try
            {
                if (dTO.UserId == Guid.Empty)
                {
                    return BadRequest("User ID cannot be empty");
                }

                var reports = await userReportQueryService.GetUserReportsAsync(dTO.UserId);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

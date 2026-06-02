using Mdaresna.Doamin.MainDB.Enums;
using Mdaresna.Doamin.MainDB.Models;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.MainDB.Helpers;
using Mdaresna.Repository.MainDB.DTOs;
using Mdaresna.Repository.MainDB.IServices;
using Mdaresna.Repository.MainDB.IUnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Mdaresna.Infrastructure.MainDB.Services;

internal class MdaresnaSchoolService : IMdaresnaSchoolService
{
    private readonly IMainUnitOfWork unitOfWork;

    public MdaresnaSchoolService(IMainUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }
    public async Task<bool> ChangeSchoolActivation(Guid schoolId, bool isActive)
    {
        var school = await unitOfWork.MdaresnaSchool.GetByIdAsync(default, schoolId);

        if (school == null)
        {
            return false;
        }

        school.IsActive = isActive;

        unitOfWork.MdaresnaSchool.Update(school);

        await unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CreateSchoolAsync(Guid schoolId, string schoolName, List<CreateSchoolServiceDTO> schoolServicesList)
    {
        await unitOfWork.BeginTransactionAsync();

        try
        {
            await AddSchool(schoolId, schoolName);

            await AddSchoolServices(schoolId, schoolServicesList);

            await unitOfWork.SaveChangesAsync();

            await unitOfWork.CommitTransactionAsync();

            return true;



        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            throw ex;
        }
    }

    private async Task AddSchool(Guid schoolId, string schoolName)
    {
        var school = new MdaresnaSchool
        {
            Id = schoolId,
            Name = schoolName,
            DBSource = "185.112.200.82",
            DBPassword = "123",
            DBType = DBTypeEnum.SQLServer,
            DBUser = "user",
            DBCatlog = "MdaresnaPreProd",
            IsActive = false,
            CreateDate = DateTime.UtcNow,
            LastModifyDate = DateTime.UtcNow,
            Deleted = false
        };

        await unitOfWork.MdaresnaSchool.AddAsync(school);
    }

    private async Task AddSchoolServices(Guid schoolId, List<CreateSchoolServiceDTO> schoolServicesList)
    {
        var servicesIds = schoolServicesList.Select(s=> s.ServiceId).ToList();
        var servicesList = await unitOfWork.MdaresnaService.Query().Where(e => servicesIds.Contains(e.Id) && e.IsActive && !e.Deleted).ToListAsync();

        if (servicesList.IsNullOrEmpty())
            throw new Exception("Services not implemented");
        foreach (var schoolService in schoolServicesList)
        {
            var selectedService = servicesList.FirstOrDefault(s => s.Id == schoolService.ServiceId);
            if (selectedService == null)
                continue;

            var newSchoolService = new Mdaresna.Doamin.MainDB.Models.MdaresnaSchoolService
            {
                SchoolId = schoolId,
                ServiceId = selectedService.Id,
                DBType = selectedService.DBType,
                DBSource = selectedService.DBSource,
                DBPort = selectedService.DBPort,
                DBUser = selectedService.DBUser,
                DBPassword = DBHelper.GenerateDbPassword(12),
                DBCatlog = $"{schoolId}_{selectedService.Name}",

            };

            await unitOfWork.MdaresnaSchoolService.AddAsync(newSchoolService);
        }
    }
}

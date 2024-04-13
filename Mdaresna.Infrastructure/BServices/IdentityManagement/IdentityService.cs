using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Repository.Helpers;
using Mdaresna.Repository.IBServices.IdentityManagement;
using Mdaresna.Repository.IServices.SettingsManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Command;
using Mdaresna.Repository.IServices.UserManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.BServices.IdentityManagement
{
    public class IdentityService : IIdentityService
    {
        private readonly IUserCommandService userCommandService;
        private readonly IUserQueryService userQueryService;
        private readonly ISMSProviderQueryService sMSProviderQueryService;

        public IdentityService(IUserCommandService userCommandService,
                                IUserQueryService userQueryService,
                                ISMSProviderQueryService sMSProviderQueryService)
        {
            this.userCommandService = userCommandService;
            this.userQueryService = userQueryService;
            this.sMSProviderQueryService = sMSProviderQueryService;
        }

        public async Task<RegisterResultDTO> Register(User RegisterUser)
        {
            var result = new RegisterResultDTO { MSG = string.Empty };
          
            var user = await userQueryService.GetUserByPhoneNumber(RegisterUser.PhoneNumber);
            if (user != null)
            {
                result.MSG = "User Already Exist";
                result.Regidterd = false;
                return result;
            }

            RegisterUser.PhoneConfirmationCode = SMSHelper.GenerateConfirmationKey();
            result.Regidterd = userCommandService.Create(RegisterUser);
            result.MSG = await SendConferamtionKey(RegisterUser);
            return result;
        }

        private async Task<string> SendConferamtionKey(User user)
        {
            var smsProvider = await sMSProviderQueryService.GetFirstActive();
            string MSG;
            MSG = await SMSHelper.SendConfirmationKey(smsProvider, user);
            return MSG;
        }
    }
}

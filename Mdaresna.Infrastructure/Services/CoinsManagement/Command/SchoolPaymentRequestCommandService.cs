using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.CoinsManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.CoinsManagement.Command
{
    public class SchoolPaymentRequestCommandService : IBaseCommandService<SchoolPaymentRequest>, ISchoolPaymentRequestCommandService
    {
        private readonly IBaseCommandRepository<SchoolPaymentRequest> commandRepository;
        private readonly IBaseSharedRepository<SchoolPaymentRequest> sharedRepository;

        public SchoolPaymentRequestCommandService(IBaseCommandRepository<SchoolPaymentRequest> commandRepository,
            IBaseSharedRepository<SchoolPaymentRequest> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(SchoolPaymentRequest entity)
        {
            try
            {
                entity.Id = DataGenerationHelper.GenerateRowId();
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(SchoolPaymentRequest entity)
        {
            try
            {
                entity = await sharedRepository.GetAsync(entity.Id);
                return commandRepository.Delete(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Update(SchoolPaymentRequest entity)
        {
            try
            {
                return commandRepository.Update(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

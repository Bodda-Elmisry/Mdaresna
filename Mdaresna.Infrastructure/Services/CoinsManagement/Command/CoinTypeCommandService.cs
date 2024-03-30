using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Infrastructure.Hilpers;
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
    public class CoinTypeCommandService : IBaseCommandService<CoinType>, ICoinTypeCommandService
    {
        private readonly IBaseCommandRepository<CoinType> commandRepository;
        private readonly IBaseSharedRepository<CoinType> sharedRepository;

        public CoinTypeCommandService(IBaseCommandRepository<CoinType> commandRepository,
            IBaseSharedRepository<CoinType> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(CoinType entity)
        {
            try
            {
                entity.Id = DataGenerationHilper.GenerateRowId();
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(CoinType entity)
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

        public bool Update(CoinType entity)
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

using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SettingsManagement.Command;
using Mdaresna.Repository.IServices.SettingsManagement.Command;

namespace Mdaresna.Infrastructure.Services.SettingsManagement.Command
{
    public class ReportQueueCommandService : IReportQueueCommandService
    {
        private readonly IReportQueueCommandRepository reportQueueCommandRepository;
        private readonly IBaseSharedRepository<ReportQueue> sharedRepository;

        public ReportQueueCommandService(
            IReportQueueCommandRepository reportQueueCommandRepository,
            IBaseSharedRepository<ReportQueue> sharedRepository)
        {
            this.reportQueueCommandRepository = reportQueueCommandRepository;
            this.sharedRepository = sharedRepository;
        }

        public bool Create(ReportQueue entity)
        {
            entity.Id = DataGenerationHelper.GenerateRowId();
            entity.CreatedAt = DateTime.UtcNow;
            entity.Status = ReportQueueStatusEnum.Queued;
            entity.RetryCount = 0;
            entity.StartedAt = null;
            entity.CompletedAt = null;
            entity.Errors = null;

            return reportQueueCommandRepository.Create(entity);
        }

        public async Task<bool> DeleteAsync(ReportQueue entity)
        {
            var reportQueue = await sharedRepository.GetAsync(entity.Id);

            if (reportQueue == null)
            {
                return false;
            }

            return reportQueueCommandRepository.Delete(reportQueue);
        }

        public bool Update(ReportQueue entity)
        {
            return reportQueueCommandRepository.Update(entity);
        }

        public async Task<bool> MarkStartedAsync(Guid id)
        {
            var reportQueue = await sharedRepository.GetAsync(id);

            if (reportQueue == null)
            {
                return false;
            }

            reportQueue.Status = ReportQueueStatusEnum.Processing;
            reportQueue.StartedAt ??= DateTime.UtcNow;
            reportQueue.CompletedAt = null;

            return reportQueueCommandRepository.Update(reportQueue);
        }

        public async Task<bool> MarkCompletedAsync(Guid id, int? affectedRows = null, string? notes = null)
        {
            var reportQueue = await sharedRepository.GetAsync(id);

            if (reportQueue == null)
            {
                return false;
            }

            reportQueue.Status = ReportQueueStatusEnum.PendingReview;
            reportQueue.StartedAt ??= DateTime.UtcNow;
            reportQueue.CompletedAt = DateTime.UtcNow;
            reportQueue.Errors = null;
            reportQueue.AffectedRows = affectedRows;
            reportQueue.Notes = notes;

            return reportQueueCommandRepository.Update(reportQueue);
        }

        public async Task<bool> MarkFailedAsync(
            Guid id,
            string errors,
            int? affectedRows = null,
            string? notes = null)
        {
            var reportQueue = await sharedRepository.GetAsync(id);

            if (reportQueue == null)
            {
                return false;
            }

            reportQueue.Status = ReportQueueStatusEnum.Failed;
            reportQueue.StartedAt ??= DateTime.UtcNow;
            reportQueue.CompletedAt = DateTime.UtcNow;
            reportQueue.Errors = errors;
            reportQueue.AffectedRows = affectedRows;
            reportQueue.RetryCount += 1;
            reportQueue.Notes = notes;

            return reportQueueCommandRepository.Update(reportQueue);
        }
    }
}

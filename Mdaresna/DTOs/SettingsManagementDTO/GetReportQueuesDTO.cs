using Mdaresna.Doamin.Enums;
using Mdaresna.DTOs.Common;

namespace Mdaresna.DTOs.SettingsManagementDTO
{
    public class GetReportQueuesDTO : SchoolIdDTO
    {
        public Guid? MonthId { get; set; }

        public Guid? GradeId { get; set; }

        public Guid? ClassroomId { get; set; }

        public ReportQueueStatusEnum? Status { get; set; }

        public StudentReportTypesEnum? ReportType { get; set; }
    }
}

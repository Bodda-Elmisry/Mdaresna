using Mdaresna.Doamin.DTOs.CoinsManagement;
using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.CoinsManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.CoinsManagement.Query
{
    public class SchoolPaymentRequestQueryRepository : BaseQueryRepository<SchoolPaymentRequest>, ISchoolPaymentRequestQueryRepository
    {
        private readonly AppDbContext context;

        public SchoolPaymentRequestQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<SchoolPaymentRequestResultDTO>> GetSchoolPaymentRequestsListAsync(DateTime? requestDateFrom, DateTime? requestDateTo, string? transfareCode, DateTime? transfareDateFrom, DateTime? transfareDateTo, 
                                                                                                            decimal? transfareAmount, Guid? paymentTypeId, Guid? schoolId, bool? approvied, Guid? approviedById)
        {
            var query = context.SchoolPaymentRequests.AsQueryable();

            requestDateFrom = requestDateFrom == null ? null : new DateTime(requestDateFrom.Value.Year, requestDateFrom.Value.Month, requestDateFrom.Value.Day);
            requestDateTo = requestDateTo == null ? null : new DateTime(requestDateTo.Value.Year, requestDateTo.Value.Month, requestDateTo.Value.Day);

            if(requestDateFrom != null && requestDateTo != null)
                query = query.Where(r=> r.RequestDate >= requestDateFrom && r.RequestDate <= requestDateTo);
            else if(requestDateFrom != null)
                query = query.Where(r=> r.RequestDate == requestDateFrom);

            if(!string.IsNullOrEmpty(transfareCode))
                query = query.Where(r=> r.TransfareCode.Contains(transfareCode));

            transfareDateFrom = transfareDateFrom == null ? null : new DateTime(transfareDateFrom.Value.Year, transfareDateFrom.Value.Month, transfareDateFrom.Value.Day);
            transfareDateTo = transfareDateTo == null ? null : new DateTime(transfareDateTo.Value.Year, transfareDateTo.Value.Month, transfareDateTo.Value.Day);

            if (transfareDateFrom != null && transfareDateTo != null)
                query = query.Where(r => r.TransfareDate >= transfareDateFrom && r.TransfareDate <= transfareDateTo);
            else if (transfareDateFrom != null)
                query = query.Where(r => r.RequestDate == transfareDateFrom);

            if(transfareAmount != null)
                query = query.Where(r=> r.TransfareAmount == transfareAmount);

            if (paymentTypeId != null)
                query = query.Where(r => r.PaymentTypeId == paymentTypeId);

            if (schoolId != null)
                query = query.Where(r => r.SchoolId == schoolId);

            if (approvied != null)
                query = query.Where(r => r.Approvied == approvied);

            if (approviedById != null)
                query = query.Where(r => r.ApproviedById == approviedById);


            query = query.Include(r => r.paymentType)
                         .Include(r => r.School)
                         .Include(r => r.ApproviedBy).OrderByDescending(r=> r.RequestDate);

            return await query.Select(r => new SchoolPaymentRequestResultDTO
            {
                Id = r.Id,
                RequestDate = r.RequestDate,
                TransfareCode = r.TransfareCode,
                TransfareDate = r.TransfareDate,
                TransfareAmount = r.TransfareAmount,
                PaymentTypeId = r.PaymentTypeId,
                PaymentTypeName = r.paymentType.Name,
                SchoolId = r.SchoolId,
                SchoolName = r.School.Name,
                Approvied = r.Approvied,
                ApproviedById = r.ApproviedById,
                ApproviedByName = $"{r.ApproviedBy.FirstName} {r.ApproviedBy.MiddelName} {r.ApproviedBy.LastName}"
            }).ToListAsync();

        }


        public async Task<SchoolPaymentRequestResultDTO?> GetSchoolPaymentRequestViewAsync(Guid id)
        {
            var request = await context.SchoolPaymentRequests.Include(r => r.paymentType)
                                                             .Include(r => r.School)
                                                             .Include(r => r.ApproviedBy)
                                                             .FirstOrDefaultAsync(r=> r.Id == id);

            return request == null ? null : new SchoolPaymentRequestResultDTO
            {
                Id = request.Id,
                RequestDate = request.RequestDate,
                TransfareCode = request.TransfareCode,
                TransfareDate = request.TransfareDate,
                TransfareAmount = request.TransfareAmount,
                PaymentTypeId = request.PaymentTypeId,
                PaymentTypeName = request.paymentType.Name,
                SchoolId = request.SchoolId,
                SchoolName = request.School.Name,
                Approvied = request.Approvied,
                ApproviedById = request.ApproviedById,
                ApproviedByName = $"{request.ApproviedBy.FirstName} {request.ApproviedBy.MiddelName} {request.ApproviedBy.LastName}"
            };
        }




    }                                                                                                                                                       
}                                                                                                                                                           
                                                                                                                                                            
                                                                                                                                                            
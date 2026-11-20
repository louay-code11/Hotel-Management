using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HotelManagement.Invoices;

public interface IInvoiceAppService : IApplicationService
{
    Task<InvoiceDto> GetAsync(Guid id);
    Task<PagedResultDto<InvoiceDto>> GetListAsync(GetInvoiceListDto input);
    Task<InvoiceDto> GenerateForStayAsync(Guid stayId);
    Task<InvoiceDto> IssueAsync(Guid id);
    Task<InvoiceDto> VoidAsync(Guid id);
    Task<PaymentDto> RecordPaymentAsync(Guid id, RecordPaymentDto input);
}

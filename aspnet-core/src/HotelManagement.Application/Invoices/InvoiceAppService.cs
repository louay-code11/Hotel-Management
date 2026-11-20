using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagement.Enums;
using HotelManagement.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace HotelManagement.Invoices;

[Authorize(HotelManagementPermissions.Invoices.Default)]
public class InvoiceAppService : ApplicationService, IInvoiceAppService
{
    private readonly IRepository<Invoice, Guid> _invoiceRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly InvoiceManager _invoiceManager;

    public InvoiceAppService(
        IRepository<Invoice, Guid> invoiceRepository,
        IRepository<Payment, Guid> paymentRepository,
        InvoiceManager invoiceManager)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
        _invoiceManager = invoiceManager;
    }

    public async Task<InvoiceDto> GetAsync(Guid id)
    {
        var invoice = await _invoiceRepository.GetAsync(id);
        var dto = ObjectMapper.Map<Invoice, InvoiceDto>(invoice);

        var payments = await _paymentRepository.GetListAsync(p => p.InvoiceId == id);
        dto.Payments = ObjectMapper.Map<List<Payment>, List<PaymentDto>>(payments);
        dto.PaidAmount = payments.Sum(p => p.Amount);

        return dto;
    }

    public async Task<PagedResultDto<InvoiceDto>> GetListAsync(GetInvoiceListDto input)
    {
        var query = await _invoiceRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(i => i.InvoiceNumber.Contains(input.Filter));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(i => i.Status == input.Status.Value);
        }

        if (input.StayId.HasValue)
        {
            query = query.Where(i => i.StayId == input.StayId.Value);
        }

        if (input.FromDate.HasValue)
        {
            query = query.Where(i => i.IssuedAt >= input.FromDate.Value);
        }

        if (input.ToDate.HasValue)
        {
            query = query.Where(i => i.IssuedAt <= input.ToDate.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        query = query
            .OrderByDescending(i => i.IssuedAt)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var invoices = await AsyncExecuter.ToListAsync(query);
        var dtos = ObjectMapper.Map<List<Invoice>, List<InvoiceDto>>(invoices);

        var invoiceIds = invoices.Select(i => i.Id).ToList();
        var allPayments = await _paymentRepository.GetListAsync(p => invoiceIds.Contains(p.InvoiceId));

        foreach (var dto in dtos)
        {
            dto.PaidAmount = allPayments.Where(p => p.InvoiceId == dto.Id).Sum(p => p.Amount);
        }

        return new PagedResultDto<InvoiceDto>(totalCount, dtos);
    }

    [Authorize(HotelManagementPermissions.Invoices.Create)]
    public async Task<InvoiceDto> GenerateForStayAsync(Guid stayId)
    {
        var invoice = await _invoiceManager.GenerateForStayAsync(stayId);
        return await GetAsync(invoice.Id);
    }

    [Authorize(HotelManagementPermissions.Invoices.Issue)]
    public async Task<InvoiceDto> IssueAsync(Guid id)
    {
        var invoice = await _invoiceRepository.GetAsync(id);
        _invoiceManager.EnsureInvoiceIsEditable(invoice);

        invoice.Status = InvoiceStatus.Issued;
        await _invoiceRepository.UpdateAsync(invoice, autoSave: true);

        return await GetAsync(id);
    }

    [Authorize(HotelManagementPermissions.Invoices.Void)]
    public async Task<InvoiceDto> VoidAsync(Guid id)
    {
        var invoice = await _invoiceManager.VoidInvoiceAsync(id);
        return await GetAsync(invoice.Id);
    }

    [Authorize(HotelManagementPermissions.Invoices.RecordPayment)]
    public async Task<PaymentDto> RecordPaymentAsync(Guid id, RecordPaymentDto input)
    {
        var payment = await _invoiceManager.RecordPaymentAsync(
            id,
            input.Amount,
            input.PaymentMethod,
            input.TransactionRef
        );

        return ObjectMapper.Map<Payment, PaymentDto>(payment);
    }
}

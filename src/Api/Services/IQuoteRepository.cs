using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.Api.Services;

public interface IQuoteRepository
{
    Task<Quote?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Quote> CreateAsync(CreateQuoteRequest request, CancellationToken ct = default);
}

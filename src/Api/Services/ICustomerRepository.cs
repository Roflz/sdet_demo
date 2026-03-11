using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.Api.Services;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Customer> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default);
}

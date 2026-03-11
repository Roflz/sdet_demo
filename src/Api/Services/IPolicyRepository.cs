using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.Api.Services;

public interface IPolicyRepository
{
    Task<Policy?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Policy> CreateAsync(CreatePolicyRequest request, CancellationToken ct = default);
    Task<bool> UpdateStatusAsync(int id, string status, CancellationToken ct = default);
}

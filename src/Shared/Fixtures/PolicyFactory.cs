using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.Shared.Fixtures;

public static class PolicyFactory
{
    private static int _seed = 1;

    public static CreatePolicyRequest Create(int customerId, Action<CreatePolicyRequest>? overrideAction = null)
    {
        var policy = new CreatePolicyRequest
        {
            CustomerId = customerId,
            PolicyNumber = $"POL-{_seed:D6}",
            EffectiveDate = DateTime.UtcNow.ToString("yyyy-MM-dd")
        };
        _seed++;
        overrideAction?.Invoke(policy);
        return policy;
    }

    public static void ResetSeed() => _seed = 1;
}

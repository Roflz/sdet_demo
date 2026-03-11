using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.Shared.Fixtures;

public static class CustomerFactory
{
    private static int _seed = 1;

    public static CreateCustomerRequest Create(Action<CreateCustomerRequest>? overrideAction = null)
    {
        var customer = new CreateCustomerRequest
        {
            FirstName = $"TestFirst_{_seed}",
            LastName = $"TestLast_{_seed}",
            Email = $"test{_seed}@example.com",
            Phone = "555-0100"
        };
        _seed++;
        overrideAction?.Invoke(customer);
        return customer;
    }

    public static void ResetSeed() => _seed = 1;
}

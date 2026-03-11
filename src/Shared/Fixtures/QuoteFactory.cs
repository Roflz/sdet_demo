using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.Shared.Fixtures;

public static class QuoteFactory
{
    private static int _seed = 1;

    public static CreateQuoteRequest Create(int customerId, Action<CreateQuoteRequest>? overrideAction = null)
    {
        var quote = new CreateQuoteRequest
        {
            CustomerId = customerId,
            ProductCode = $"AUTO-{_seed}",
            Premium = 250.00m + _seed
        };
        _seed++;
        overrideAction?.Invoke(quote);
        return quote;
    }

    public static void ResetSeed() => _seed = 1;
}

namespace InsuranceAutomationDemo.Shared.Models;

public class Policy
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string PolicyNumber { get; set; } = "";
    public string Status { get; set; } = ""; // e.g. Active, Cancelled, Pending
    public string? EffectiveDate { get; set; }
}

public class CreatePolicyRequest
{
    public int CustomerId { get; set; }
    public string PolicyNumber { get; set; } = "";
    public string? EffectiveDate { get; set; }
}

public class UpdatePolicyStatusRequest
{
    public string Status { get; set; } = "";
}

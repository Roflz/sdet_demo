namespace InsuranceAutomationDemo.Shared.Models;

public class Quote
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string? ProductCode { get; set; }
    public decimal Premium { get; set; }
    public string Status { get; set; } = ""; // e.g. Draft, Accepted, Expired
}

public class CreateQuoteRequest
{
    public int CustomerId { get; set; }
    public string? ProductCode { get; set; }
    public decimal Premium { get; set; }
}

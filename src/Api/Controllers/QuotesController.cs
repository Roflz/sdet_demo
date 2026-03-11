using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsuranceAutomationDemo.Shared.Models;
using InsuranceAutomationDemo.Api.Services;

namespace InsuranceAutomationDemo.Api.Controllers;

[ApiController]
[Route("quotes")]
[Authorize(Policy = "ApiKey")]
public class QuotesController : ControllerBase
{
    private readonly IQuoteRepository _repo;

    public QuotesController(IQuoteRepository repo) => _repo = repo;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Quote>> Get(int id, CancellationToken ct)
    {
        var quote = await _repo.GetByIdAsync(id, ct);
        if (quote == null)
            return NotFound();
        return Ok(quote);
    }

    [HttpPost]
    public async Task<ActionResult<Quote>> Post([FromBody] CreateQuoteRequest request, CancellationToken ct)
    {
        if (request.CustomerId <= 0)
            return BadRequest("CustomerId is required.");
        var quote = await _repo.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = quote.Id }, quote);
    }
}

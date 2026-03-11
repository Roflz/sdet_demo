using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsuranceAutomationDemo.Shared.Models;
using InsuranceAutomationDemo.Api.Services;

namespace InsuranceAutomationDemo.Api.Controllers;

[ApiController]
[Route("customers")]
[Authorize(Policy = "ApiKey")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _repo;

    public CustomersController(ICustomerRepository repo) => _repo = repo;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Customer>> Get(int id, CancellationToken ct)
    {
        var customer = await _repo.GetByIdAsync(id, ct);
        if (customer == null)
            return NotFound();
        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Post([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("FirstName, LastName, and Email are required.");
        var customer = await _repo.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = customer.Id }, customer);
    }
}

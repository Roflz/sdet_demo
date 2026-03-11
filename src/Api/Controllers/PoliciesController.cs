using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsuranceAutomationDemo.Shared.Models;
using InsuranceAutomationDemo.Api.Services;

namespace InsuranceAutomationDemo.Api.Controllers;

[ApiController]
[Route("policies")]
[Authorize(Policy = "ApiKey")]
public class PoliciesController : ControllerBase
{
    private readonly IPolicyRepository _repo;

    public PoliciesController(IPolicyRepository repo) => _repo = repo;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Policy>> Get(int id, CancellationToken ct)
    {
        var policy = await _repo.GetByIdAsync(id, ct);
        if (policy == null)
            return NotFound();
        return Ok(policy);
    }

    [HttpPost]
    public async Task<ActionResult<Policy>> Post([FromBody] CreatePolicyRequest request, CancellationToken ct)
    {
        if (request.CustomerId <= 0 || string.IsNullOrWhiteSpace(request.PolicyNumber))
            return BadRequest("CustomerId and PolicyNumber are required.");
        var policy = await _repo.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = policy.Id }, policy);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult> PatchStatus(int id, [FromBody] UpdatePolicyStatusRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
            return BadRequest("Status is required.");
        var updated = await _repo.UpdateStatusAsync(id, request.Status, ct);
        if (!updated)
            return NotFound();
        return NoContent();
    }
}

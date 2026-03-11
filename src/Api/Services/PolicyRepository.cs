using Microsoft.Data.SqlClient;
using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.Api.Services;

public class PolicyRepository : IPolicyRepository
{
    private readonly string _connectionString;

    public PolicyRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Default") ?? "";
    }

    public async Task<Policy?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand("SELECT Id, CustomerId, PolicyNumber, Status, EffectiveDate FROM Policies WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;
        return Map(reader);
    }

    public async Task<Policy> CreateAsync(CreatePolicyRequest request, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            "INSERT INTO Policies (CustomerId, PolicyNumber, Status, EffectiveDate) OUTPUT INSERTED.Id VALUES (@cid, @num, 'Pending', @eff)", conn);
        cmd.Parameters.AddWithValue("@cid", request.CustomerId);
        cmd.Parameters.AddWithValue("@num", request.PolicyNumber);
        cmd.Parameters.AddWithValue("@eff", string.IsNullOrEmpty(request.EffectiveDate) ? DBNull.Value : request.EffectiveDate);
        var id = (int)(await cmd.ExecuteScalarAsync(ct))!;
        return new Policy
        {
            Id = id,
            CustomerId = request.CustomerId,
            PolicyNumber = request.PolicyNumber,
            Status = "Pending",
            EffectiveDate = request.EffectiveDate
        };
    }

    public async Task<bool> UpdateStatusAsync(int id, string status, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand("UPDATE Policies SET Status = @status WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@status", status);
        cmd.Parameters.AddWithValue("@id", id);
        return await cmd.ExecuteNonQueryAsync(ct) > 0;
    }

    private static Policy Map(SqlDataReader r)
    {
        return new Policy
        {
            Id = r.GetInt32(0),
            CustomerId = r.GetInt32(1),
            PolicyNumber = r.GetString(2),
            Status = r.GetString(3),
            EffectiveDate = r.IsDBNull(4) ? null : r.GetDateTime(4).ToString("yyyy-MM-dd")
        };
    }
}

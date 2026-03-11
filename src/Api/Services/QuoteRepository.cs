using Microsoft.Data.SqlClient;
using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.Api.Services;

public class QuoteRepository : IQuoteRepository
{
    private readonly string _connectionString;

    public QuoteRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Default") ?? "";
    }

    public async Task<Quote?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand("SELECT Id, CustomerId, ProductCode, Premium, Status FROM Quotes WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;
        return Map(reader);
    }

    public async Task<Quote> CreateAsync(CreateQuoteRequest request, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            "INSERT INTO Quotes (CustomerId, ProductCode, Premium, Status) OUTPUT INSERTED.Id VALUES (@cid, @code, @prem, 'Draft')", conn);
        cmd.Parameters.AddWithValue("@cid", request.CustomerId);
        cmd.Parameters.AddWithValue("@code", (object?)request.ProductCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@prem", request.Premium);
        var id = (int)(await cmd.ExecuteScalarAsync(ct))!;
        return new Quote
        {
            Id = id,
            CustomerId = request.CustomerId,
            ProductCode = request.ProductCode,
            Premium = request.Premium,
            Status = "Draft"
        };
    }

    private static Quote Map(SqlDataReader r)
    {
        return new Quote
        {
            Id = r.GetInt32(0),
            CustomerId = r.GetInt32(1),
            ProductCode = r.IsDBNull(2) ? null : r.GetString(2),
            Premium = r.GetDecimal(3),
            Status = r.GetString(4)
        };
    }
}

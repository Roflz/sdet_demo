using Microsoft.Data.SqlClient;
using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.Api.Services;

public class CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Default") ?? "";
    }

    public async Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand("SELECT Id, FirstName, LastName, Email, Phone FROM Customers WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;
        return Map(reader);
    }

    public async Task<Customer> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(
            "INSERT INTO Customers (FirstName, LastName, Email, Phone) OUTPUT INSERTED.Id VALUES (@fn, @ln, @email, @phone)", conn);
        cmd.Parameters.AddWithValue("@fn", request.FirstName);
        cmd.Parameters.AddWithValue("@ln", request.LastName);
        cmd.Parameters.AddWithValue("@email", request.Email);
        cmd.Parameters.AddWithValue("@phone", (object?)request.Phone ?? DBNull.Value);
        var id = (int)(await cmd.ExecuteScalarAsync(ct))!;
        return new Customer
        {
            Id = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone
        };
    }

    private static Customer Map(SqlDataReader r)
    {
        return new Customer
        {
            Id = r.GetInt32(0),
            FirstName = r.GetString(1),
            LastName = r.GetString(2),
            Email = r.GetString(3),
            Phone = r.IsDBNull(4) ? null : r.GetString(4)
        };
    }
}

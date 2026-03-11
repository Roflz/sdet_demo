using Microsoft.Data.SqlClient;

namespace InsuranceAutomationDemo.Shared.Database;

public class DbHelper : IAsyncDisposable
{
    private readonly string _connectionString;

    public DbHelper(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<T?> ExecuteScalarAsync<T>(string sql, CancellationToken ct = default, params SqlParameter[] parameters)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        var result = await cmd.ExecuteScalarAsync(ct);
        return result is null or DBNull ? default : (T)Convert.ChangeType(result, typeof(T));
    }

    public async Task<int> ExecuteNonQueryAsync(string sql, CancellationToken ct = default, params SqlParameter[] parameters)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        return await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<T?> QuerySingleAsync<T>(string sql, Func<SqlDataReader, T> map, CancellationToken ct = default, params SqlParameter[] parameters)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return default;
        return map(reader);
    }

    public async Task<bool> RecordExistsAsync(string table, string idColumn, object idValue, CancellationToken ct = default)
    {
        var sql = $"SELECT COUNT(*) FROM {table} WHERE {idColumn} = @id";
        var count = await ExecuteScalarAsync<int>(sql, ct, new SqlParameter("@id", idValue));
        return count > 0;
    }

    public async Task<string?> GetPolicyStatusAsync(int policyId, CancellationToken ct = default)
    {
        const string sql = "SELECT Status FROM Policies WHERE Id = @id";
        return await ExecuteScalarAsync<string>(sql, ct, new SqlParameter("@id", policyId));
    }

    public async Task<QuoteRow?> GetQuoteByIdAsync(int quoteId, CancellationToken ct = default)
    {
        const string sql = "SELECT Id, CustomerId, ProductCode, Premium, Status FROM Quotes WHERE Id = @id";
        return await QuerySingleAsync(sql, r => new QuoteRow
        {
            Id = r.GetInt32(0),
            CustomerId = r.GetInt32(1),
            ProductCode = r.IsDBNull(2) ? null : r.GetString(2),
            Premium = r.GetDecimal(3),
            Status = r.GetString(4)
        }, ct, new SqlParameter("@id", quoteId));
    }

    public async Task<int> GetCustomerPolicyCountAsync(int customerId, CancellationToken ct = default)
    {
        const string sql = "SELECT COUNT(*) FROM Policies WHERE CustomerId = @customerId";
        int? count = await ExecuteScalarAsync<int>(sql, ct, new SqlParameter("@customerId", customerId));
        return count ?? 0;
    }

    public async Task<CustomerPolicyRow?> GetCustomerWithPolicyAsync(int customerId, int policyId, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT c.Id, c.FirstName, c.LastName, p.Id, p.PolicyNumber, p.Status
            FROM Customers c
            INNER JOIN Policies p ON p.CustomerId = c.Id
            WHERE c.Id = @customerId AND p.Id = @policyId";
        return await QuerySingleAsync(sql, r => new CustomerPolicyRow
        {
            CustomerId = r.GetInt32(0),
            FirstName = r.GetString(1),
            LastName = r.GetString(2),
            PolicyId = r.GetInt32(3),
            PolicyNumber = r.GetString(4),
            PolicyStatus = r.GetString(5)
        }, ct, new SqlParameter("@customerId", customerId), new SqlParameter("@policyId", policyId));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

public class QuoteRow
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string? ProductCode { get; set; }
    public decimal Premium { get; set; }
    public string Status { get; set; } = "";
}

public class CustomerPolicyRow
{
    public int CustomerId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public int PolicyId { get; set; }
    public string PolicyNumber { get; set; } = "";
    public string PolicyStatus { get; set; } = "";
}

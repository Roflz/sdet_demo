using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using InsuranceAutomationDemo.Shared.Config;
using InsuranceAutomationDemo.Shared.Models;

namespace InsuranceAutomationDemo.Shared.Clients;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient http, TestConfig config)
    {
        _http = http;
        _http.BaseAddress = new Uri(config.BaseApiUrl.TrimEnd('/') + "/");
        if (!string.IsNullOrEmpty(config.AuthToken))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.AuthToken);
    }

    public async Task<HttpResponseMessage> PostCustomerAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        return await _http.PostAsync("customers", new StringContent(json, Encoding.UTF8, "application/json"), ct);
    }

    public async Task<HttpResponseMessage> GetCustomerAsync(int id, CancellationToken ct = default)
    {
        return await _http.GetAsync($"customers/{id}", ct);
    }

    public async Task<HttpResponseMessage> PostPolicyAsync(CreatePolicyRequest request, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        return await _http.PostAsync("policies", new StringContent(json, Encoding.UTF8, "application/json"), ct);
    }

    public async Task<HttpResponseMessage> GetPolicyAsync(int id, CancellationToken ct = default)
    {
        return await _http.GetAsync($"policies/{id}", ct);
    }

    public async Task<HttpResponseMessage> PatchPolicyStatusAsync(int id, UpdatePolicyStatusRequest request, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        return await _http.PatchAsync($"policies/{id}/status", new StringContent(json, Encoding.UTF8, "application/json"), ct);
    }

    public async Task<HttpResponseMessage> PostQuoteAsync(CreateQuoteRequest request, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        return await _http.PostAsync("quotes", new StringContent(json, Encoding.UTF8, "application/json"), ct);
    }

    public async Task<HttpResponseMessage> GetQuoteAsync(int id, CancellationToken ct = default)
    {
        return await _http.GetAsync($"quotes/{id}", ct);
    }

    public async Task<T?> ReadAsJsonAsync<T>(HttpResponseMessage response, CancellationToken ct = default)
    {
        var content = await response.Content.ReadAsStringAsync(ct);
        return string.IsNullOrEmpty(content) ? default : JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }
}

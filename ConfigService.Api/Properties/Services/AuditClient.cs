public interface IAuditService
{
    Task RecordConfigChangeAsync(string service, string key, string value);
}

public class AuditService : IAuditService
{
    private readonly HttpClient _client;

    public AuditService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("AuditClient");
    }

    public async Task RecordConfigChangeAsync(string service, string key, string value)
    {
        var payload = new
        {
            service,
            key,
            value,
            changedAt = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/audit/config-change", payload);
        response.EnsureSuccessStatusCode();
    }
}

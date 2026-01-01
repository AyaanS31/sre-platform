using Microsoft.AspNetCore.Mvc;

namespace ConfigService.Api.Controllers;


[ApiController]
[Route("config")]
public class ConfigController : ControllerBase
{
    private readonly IDictionary<string, string> _store;
    private readonly IAuditService _auditService;
    public ConfigController(IDictionary<string, string> store, IAuditService auditService)
    {
        _store = store;
        _auditService = auditService;
    }

    [HttpGet("{key}")]
    public IActionResult GetConfig(string key)
    {
        if (_store.ContainsKey(key))
            return Ok(new { key, value = _store[key] });

        return NotFound(new { message = "Key not found" });
    }

    [HttpPost]
    public IActionResult SetConfig([FromBody] Dictionary<string, string> data)
    {
        foreach (var item in data)
        {
            _store[item.Key] = item.Value;
            _auditService.RecordConfigChangeAsync("ConfigService", item.Key, item.Value);
        }

        return Ok(new { message = "Config updated", data });
    }

    [HttpGet("chaos")]
    public IActionResult Chaos([FromQuery] string type = "latency")
    {
        if (type == "latency")
        {
            Thread.Sleep(8000); // 8 seconds - triggers timeout
            return Ok(new { message = "Slow response" });
        }

        if (type == "exception")
        {
            throw new Exception("Simulated failure");
        }

        return Ok(new { message = "No chaos triggered" });
    }

}

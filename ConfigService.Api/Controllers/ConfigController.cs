using Microsoft.AspNetCore.Mvc;

namespace ConfigService.Api.Controllers;


[ApiController]
[Route("config")]
public class ConfigController : ControllerBase
{
    private readonly IDictionary<string, string> _store;

    public ConfigController(IDictionary<string, string> store)
    {
        _store = store;
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
        }

        return Ok(new { message = "Config updated", data });
    }
}

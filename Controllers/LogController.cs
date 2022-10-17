using Microsoft.AspNetCore.Mvc;

namespace AspNet_6_GrayLog.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LogController : Controller
{
    private ILogger<LogController> _logger;

    public LogController(ILogger<LogController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get(string data)
    {
        _logger.LogInformation($"Log Information {data}");
        _logger.LogWarning($"Log Warning {data}");
        _logger.LogError($"Log Error {data}");
        _logger.LogCritical($"Log Critical {data}");

        return Ok();
    }
}
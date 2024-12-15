using Microsoft.AspNetCore.Mvc;

namespace Backstage.Controllers
{
    [ApiController]
    [Route("levels")]
    public class Levels : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<Levels> _logger;

        public Levels(ILogger<Levels> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            
        }
    }
}

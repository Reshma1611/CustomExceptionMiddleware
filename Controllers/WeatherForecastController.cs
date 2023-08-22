using Microsoft.AspNetCore.Mvc;

namespace CustomExceptionMiddleware.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
        [HttpGet("GetByName")]
        public async Task<IActionResult> GetByName(string name)
        {
            var result = Summaries.FirstOrDefault(x => x == name);
            if (result == null)
            {
                throw new ApplicationError($"{name} not found");
            }
            else
            {
                return new JsonResult(result);
            }
        }

        [HttpGet("UpdateByName")]
        public async Task<IActionResult> UpdateByName(string oldName, string newName)
        {
            if (oldName == null)
            {
                throw new ApplicationError(nameof(oldName), $"{nameof(oldName)} is required");
            }
            string result = Summaries.FirstOrDefault(x => x == oldName) ?? string.Empty;


            if (result == string.Empty)
            {
                throw new ApplicationError(System.Net.HttpStatusCode.NotFound, $"{oldName} not found");
            }
            else
            {
                Summaries[Array.IndexOf(Summaries, oldName)] = newName;                
                return Ok("Updated");
            }
        }


    }
}
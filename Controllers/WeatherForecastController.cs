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
        public string[] Get()
        {
            return Summaries;       
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
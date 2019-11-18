using System.Threading.Tasks;
using AJT.Azure.Functions;
using BabouExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Startup))]
namespace AJT.Azure.Functions
{
    public class Converter
    {
        private readonly ILogger<Converter> _logger;

        public Converter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Converter>();
        }

        [FunctionName("ConvertIntToList")]
        public async Task<IActionResult> ConvertIntToList(
            [HttpTrigger(AuthorizationLevel.System, "get", "post", Route = null)]
            HttpRequest req)
        {
            _logger.LogInformation("Function ConvertIntToList processed a request.");

            var requestBody = await req.GetRawBodyStringAsync();

            if (requestBody.TryGetList(',', out var cleanString))
            {
                var stringReturn = string.Join(',', cleanString);

                return new OkObjectResult($"{stringReturn}");
            }

            return new BadRequestObjectResult($"{requestBody}");
        }

        [FunctionName("ConvertStringToList")]
        public async Task<IActionResult> ConvertStringToList(
            [HttpTrigger(AuthorizationLevel.System, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("Function ConvertStringToList processed a request.");

            var requestBody = await req.GetRawBodyStringAsync();

            if (requestBody.TryGetList(',', out var cleanString))
            {
                var stringReturn = string.Empty;

                cleanString.ForEach(x => { stringReturn += $"'{x}',"; });

                stringReturn = stringReturn.TrimEnd(',');

                return new OkObjectResult($"{stringReturn}");
            }

            return new BadRequestObjectResult($"{requestBody}");

        }
    }
}

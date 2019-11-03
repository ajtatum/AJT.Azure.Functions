using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BabouExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AJT.Azure.Functions
{
    public static class Converter
    {
        [FunctionName("ConvertIntToList")]
        public static async Task<IActionResult> ConvertIntToList(
            [HttpTrigger(AuthorizationLevel.System, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Function ConvertIntToList processed a request.");

            var requestBody = await req.GetRawBodyStringAsync();

            if (requestBody.TryGetList(',', out var cleanString))
            {
                var stringReturn = string.Join(',', cleanString);

                return new OkObjectResult($"{stringReturn}");
            }

            return new BadRequestObjectResult($"{requestBody}");
        }

        [FunctionName("ConvertStringToList")]
        public static async Task<IActionResult> ConvertStringToList(
            [HttpTrigger(AuthorizationLevel.System, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Function ConvertStringToList processed a request.");

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

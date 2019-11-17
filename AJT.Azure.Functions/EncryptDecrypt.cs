using AJT.Azure.Functions;
using BabouExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Logging;

[assembly: WebJobsStartup(typeof(Startup))]
namespace AJT.Azure.Functions
{
    public class EncryptDecrypt
    {
        private readonly ILogger<EncryptDecrypt> _logger;

        public EncryptDecrypt(ILogger<EncryptDecrypt> logger)
        {
            _logger = logger;
        }

        [FunctionName("Encrypt")]
        public IActionResult Encrypt(
            [HttpTrigger(AuthorizationLevel.System, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("Function Encrypt processed a request.");

            var originalValue = req.Headers["OriginalValue"].ToString();
            var encryptionKey = req.Headers["EncryptionKey"].ToString();

            return new OkObjectResult(originalValue.Encrypt(encryptionKey));
        }

        [FunctionName("Decrypt")]
        public IActionResult Decrypt(
            [HttpTrigger(AuthorizationLevel.System, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("Function Decrypt processed a request.");

            var originalValue = req.Headers["OriginalValue"].ToString();
            var decryptionKey = req.Headers["DecryptionKey"].ToString();

            return new OkObjectResult(originalValue.Decrypt(decryptionKey));
        }
    }
}

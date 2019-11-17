using BabouExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AJT.Azure.Functions
{
    public class EncryptDecrypt
    {
        [FunctionName("Encrypt")]
        public static IActionResult Encrypt(
            [HttpTrigger(AuthorizationLevel.System, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Function Encrypt processed a request.");

            var originalValue = req.Headers["OriginalValue"].ToString();
            var encryptionKey = req.Headers["EncryptionKey"].ToString();

            return new OkObjectResult(originalValue.Encrypt(encryptionKey));
        }

        [FunctionName("Decrypt")]
        public static IActionResult Decrypt(
            [HttpTrigger(AuthorizationLevel.System, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Function Decrypt processed a request.");

            var originalValue = req.Headers["OriginalValue"].ToString();
            var decryptionKey = req.Headers["DecryptionKey"].ToString();

            return new OkObjectResult(originalValue.Decrypt(decryptionKey));
        }
    }
}

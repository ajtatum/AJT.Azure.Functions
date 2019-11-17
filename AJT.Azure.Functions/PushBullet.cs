using System;
using System.Threading.Tasks;
using System.Web.Http;
using AJT.Azure.Functions;
using AJT.Azure.Functions.Models;
using BabouExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PushBulletSharp.Core;
using PushBulletSharp.Core.Models.Requests;

[assembly: WebJobsStartup(typeof(Startup))]
namespace AJT.Azure.Functions
{
    public class PushBullet
    {
        private readonly ILogger<PushBullet> _logger;

        public PushBullet(ILogger<PushBullet> logger)
        {
            _logger = logger;
        }

        [FunctionName("SendPushBulletFromAppVeyor")]
        public async Task<IActionResult> SendPushBulletFromAppVeyor(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req)
        {
            try
            {
                var pushBulletApiKey = Environment.GetEnvironmentVariable("PushBulletApiKey", EnvironmentVariableTarget.Process);
                var pushBulletEncryptionKey = Environment.GetEnvironmentVariable("PushBulletEncryptionKey", EnvironmentVariableTarget.Process);

                if (pushBulletApiKey.IsNullOrEmpty())
                    return new BadRequestErrorMessageResult("PushBelletApiKey cannot be found");

                if (pushBulletEncryptionKey.IsNullOrEmpty())
                    return new BadRequestErrorMessageResult("PushBulletEncryptionKey cannot be found");

                var requestBody = await req.GetRawBodyStringAsync();

                var appVeyor = JsonConvert.DeserializeObject<AppVeyor>(requestBody);

                _logger.LogInformation("AppVeyor Request Built: {@AppVeyor}", appVeyor);

                var client = new PushBulletClient(pushBulletApiKey, pushBulletEncryptionKey, TimeZoneInfo.Local);

                var channel = req.Headers["Channel"].ToString();
                var title = $"{appVeyor.EventData.ProjectName} Release";
                var body = $"There's a new version of {appVeyor.EventData.ProjectName}! Update: {appVeyor.EventData.CommitMessage}";
                var url = $"https://www.nuget.org/packages/{appVeyor.EventData.ProjectName}/{appVeyor.EventData.BuildVersion}";

                if (url.IsNullOrEmpty())
                    return new BadRequestErrorMessageResult($"Unknown url from project {appVeyor.EventData.ProjectName}");

                var pushLinkRequest = new PushLinkRequest
                {
                    ChannelTag = channel,
                    Title = title,
                    Url = url,
                    Body = body
                };

                var pushLinkResponse = await client.PushLink(pushLinkRequest);

                _logger.LogInformation("PushBullet Sent Link Message. {@PushLinkRequest} {@PushLinkResponse}", pushLinkRequest, pushLinkResponse);

                return new OkObjectResult(pushLinkResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending PushBullet message");
                return new BadRequestErrorMessageResult(ex.Message);
            }
        }
    }
}

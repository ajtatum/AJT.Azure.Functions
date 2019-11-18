using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AJT.Azure.Functions;
using BabouExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SlackBotNet;
using SlackBotNet.State;
using static SlackBotNet.MatchFactory;

[assembly: WebJobsStartup(typeof(Startup))]
namespace AJT.Azure.Functions
{
    public class Slack
    {
        private readonly ILogger<Slack> _logger;

        public Slack(ILogger<Slack> logger)
        {
            _logger = logger;
        }

        [FunctionName("StanLeeBot")]
        public async Task<IActionResult> StanLeeBot(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req)
        {
            var slackApiToken = Environment.GetEnvironmentVariable("SlackApiToken", EnvironmentVariableTarget.Process);

            var requestBody = await req.GetRawBodyStringAsync();

            _logger.LogInformation("Request Body: {RequestBody}", requestBody);

            var bot = await SlackBot.InitializeAsync(slackApiToken);

            bot.When(Matches.Text("hello"),HubType.DirectMessage | HubType.Channel, async conv =>
            {
                await conv.PostMessage($"Hi {conv.From.Username}!");
            });

            return new OkResult();
        }
    }
}

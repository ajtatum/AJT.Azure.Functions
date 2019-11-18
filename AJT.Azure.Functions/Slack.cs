using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
using SlackBotMessages;
using SlackBotMessages.Enums;
using SlackBotMessages.Models;
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
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req)
        {
            var slackApiToken = Environment.GetEnvironmentVariable("SLACK_API_TOKEN", EnvironmentVariableTarget.Process);

            var bot = await SlackBot.InitializeAsync(slackApiToken);

            bot.When(Matches.Text("hello"),HubType.DirectMessage | HubType.Channel | HubType.Group, async conv =>
            {
                await conv.PostMessage($"Hi {conv.From.Username}!");
            });

            return new OkResult();
        }

        [FunctionName("StanLeeBotCommand")]
        public async Task<IActionResult> StanLeeBotCommand(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req)
        {
            var requestForm = await req.ReadFormAsync();
            var slackCommandRequest = new SlackCommandRequest()
            {
                Token = requestForm["token"],
                TeamId = requestForm["team_id"],
                TeamDomain = requestForm["team_domain"],
                ChannelId = requestForm["channel_id"],
                ChannelName = requestForm["channel_name"],
                UserId = requestForm["user_id"],
                UserName = requestForm["user_name"],
                Command = requestForm["command"],
                Text = requestForm["text"],
                ResponseUrl = requestForm["response_url"],
                TriggerId = requestForm["trigger_id"]
            };

            _logger.LogInformation("SlackCommandRequest: {@SlackCommandRequest}", slackCommandRequest);

            switch (slackCommandRequest.Command)
            {
                case "/test":
                    await GetTest(slackCommandRequest);
                    return new OkResult();
                case "/marvel":
                    await GetMarvel(slackCommandRequest);
                    return new OkResult();
                case "/dc":
                    await GetDCComics(slackCommandRequest);
                    return new OkResult();
                default:
                    return new BadRequestResult();
            }
        }

        private async Task GetTest(SlackCommandRequest slackCommandRequest)
        {
            var client = new SbmClient(slackCommandRequest.ResponseUrl);
            var message = new Message();

            const string bio = @"I’m a techie inside and out. If it involves technology, chances are I’ve at least dabbled with it. "
                               + "I enjoy helping people and organizations learn how to leverage technology to level up their lives or organization. " 
                               + "I’ve worn many hats from having my own businesses, helping startups launch, and developing small to large scale web applications & APIs utilizing .NET & C# as the backend for many of my projects."
                               + "\n\nCombining my interests in technology and sociology, I’m simply passionate about the human side of technology… and Marvel Comics.";

            var attachment = new Attachment()
                {
                    Fallback = $"Testing for {slackCommandRequest.Text}",
                    Pretext = $"Testing for {slackCommandRequest.Text}"
                }
                .AddField("Name", "AJ Tatum", true)
                .AddField("Website", "https://ajt.io", true)
                .AddField("Bio", bio)
                .SetThumbUrl("https://ajtatum.com/wp-content/uploads/2014/08/AJ-Tatum-120x120.png")
                .SetColor(Color.Red);

            message.AddAttachment(attachment);
            await client.SendAsync(message);
        }

        private async Task GetMarvel(SlackCommandRequest slackCommandRequest)
        {
            var marvelGoogleCx = Environment.GetEnvironmentVariable("MARVEL_GOOGLE_CX", EnvironmentVariableTarget.Process);
            var gsr = await GetGoogleSearchSlackResponseJson(slackCommandRequest.Text, marvelGoogleCx);

            var client = new SbmClient(slackCommandRequest.ResponseUrl);
            var message = new Message();

            var gsrMetaTags = gsr.Items.ElementAtOrDefault(0)?.PageMap.MetaTags.ElementAtOrDefault(0) ?? new MetaTag();
            var snippet = gsr.Items.ElementAtOrDefault(0)?.Snippet.CleanString() ?? string.Empty;

            var characterName = gsrMetaTags.OgTitle.Split("|").ElementAtOrDefault(0)?.Trim();

            var attachment = new Attachment()
                {
                    Fallback = snippet,
                    Pretext = $"Excelsior! I found {characterName} :star-struck:!"
                }
                .AddField("Name", characterName, true)
                .AddField("Website", gsrMetaTags.OgUrl, true)
                .AddField("Bio", snippet)
                .SetImage(gsr.Items[0].PageMap.CseImage[0].Src)
                .SetColor(Color.Red);

            message.AddAttachment(attachment);
            await client.SendAsync(message);
        }

        private async Task GetDCComics(SlackCommandRequest slackCommandRequest)
        {
            var dcComicsCx = Environment.GetEnvironmentVariable("DC_COMICS_GOOGLE_CX", EnvironmentVariableTarget.Process);
            var gsr = await GetGoogleSearchSlackResponseJson(slackCommandRequest.Text, dcComicsCx);

            var client = new SbmClient(slackCommandRequest.ResponseUrl);
            var message = new Message();

            var gsrMetaTags = gsr.Items.ElementAtOrDefault(0)?.PageMap.MetaTags.ElementAtOrDefault(0) ?? new MetaTag();
            var snippet = gsr.Items.ElementAtOrDefault(0)?.Snippet.CleanString() ?? string.Empty;
            var characterName = gsrMetaTags.OgTitle;

            var attachment = new Attachment()
                {
                    Fallback = snippet,
                    Pretext = $"Excelsior! I found {characterName} :star-struck:!"
                }
                .AddField("Name", characterName, true)
                .AddField("Website", gsrMetaTags.OgUrl, true)
                .AddField("Bio", snippet)
                .SetThumbUrl(gsr.Items[0].PageMap.CseThumbnail.ElementAtOrDefault(0)?.Src)
                .SetColor(Color.Red);

            message.AddAttachment(attachment);
            await client.SendAsync(message);
        }

        private static async Task<GoogleSearchResponse> GetGoogleSearchSlackResponseJson(string search, string cse)
        {
            var googleApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY", EnvironmentVariableTarget.Process);

            var url = $"https://www.googleapis.com/customsearch/v1?cx={cse}&key={googleApiKey}&q={search}";

            var client = new HttpClient();
            var result = await client.GetStringAsync(url);

            var googleSearchResponse = JsonConvert.DeserializeObject<GoogleSearchResponse>(result);
            return googleSearchResponse;
        }
    }
}

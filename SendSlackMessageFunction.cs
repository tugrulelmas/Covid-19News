using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Net.Http;
using System;

namespace Notifier
{
    public static class SendSlackMessageFunction
    {
        [FunctionName("SendSlackMessage")]
        public static async Task<IActionResult> SendMessage([ActivityTrigger] IDurableActivityContext context, ILogger log) {
            var message = context.GetInput<string>();
            var client = HttpClientFactory.Create ();
            var response = await client.PostAsJsonAsync(Environment.GetEnvironmentVariable("SlackWebhookUrl"), 
                    new {
                        text = message,
                        attachments = new[] {
                        new {
                            text= $"For more details, please visit {Environment.GetEnvironmentVariable("CoronaUrl")}"
                        } }
                    });

            if (!response.IsSuccessStatusCode)
                throw new Exception (await response.Content.ReadAsStringAsync ());

            return new OkObjectResult(message);
        }
    }
}
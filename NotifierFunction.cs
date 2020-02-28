using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Notifier
{
    public static class NotifierFunction
    {
        [FunctionName("NotifierFunction")]
        public static async Task TimerStart(
            [TimerTrigger("0 */5 * * * *")]TimerInfo myTimer,
            [DurableClient]IDurableOrchestrationClient starter,
            ILogger log) {
            if (myTimer.IsPastDue)
            {
                log.LogInformation("Timer is running late!");
            }

            string instanceId = await starter.StartNewAsync("NotifierOrchestration", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
        }
        
        [FunctionName("NotifierOrchestration")]
        public static async Task<IActionResult> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var countriesSet = await context.CallActivityAsync<IDictionary<string, Country>>("GetCountriesFromDB", null);
            var parsedCountries = await context.CallActivityAsync<IEnumerable<Country>>("GetCountriesFromWeb", null);

            var messages = new StringBuilder();
            foreach (var countryItem in parsedCountries) {
                if (!countriesSet.ContainsKey(countryItem.Name)) {
                    messages.AppendLine($"The first case has been found in {countryItem.Name}");
                    await context.CallActivityAsync("CreateCountry", countryItem);
                    continue;
                }

                var needUpdate = false;
                var country = countriesSet[countryItem.Name];

                if (countryItem.NewCases > country.NewCases) {
                    var caseCount = countryItem.NewCases - country.NewCases;
                    var s = caseCount > 1 ? "s" : string.Empty;
                    messages.AppendLine($"{caseCount} new case{s} in {countryItem.Name}");
                    needUpdate = true;
                }

                if (countryItem.NewDeaths > country.NewDeaths) {
                    var deathCount = countryItem.NewDeaths - country.NewDeaths;
                    var s = deathCount > 1 ? "s" : string.Empty;
                    messages.AppendLine($"{deathCount } new death{s} in {countryItem.Name}");
                    needUpdate = true;
                }

                if (needUpdate) {
                    await context.CallActivityAsync("UpdateCountry", countryItem);
                }
            }

            var messageText = messages.ToString();
            if(!string.IsNullOrEmpty(messageText)){
                await context.CallActivityAsync("SendSlackMessage", messageText);
            }

            return new OkResult();
        }
    }
}

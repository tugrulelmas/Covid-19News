using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Net.Http;
using System;
using System.Collections.Generic;
using Polly;
using HtmlAgilityPack;
using System.Threading;

namespace Notifier
{
    public static class ParserFunction
    {
        [FunctionName("GetCountriesFromWeb")]
        public static async Task<IEnumerable<Country>> GetCountriesFromWeb([ActivityTrigger] IDurableActivityContext context, ILogger log) {
            var client = HttpClientFactory.Create ();
            var policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(new[] { 
                TimeSpan.FromSeconds(1), 
                TimeSpan.FromSeconds(2), 
                TimeSpan.FromSeconds(4) 
            });
            var html = await policy.ExecuteAsync(ct=> client.GetStringAsync(Environment.GetEnvironmentVariable("CoronaUrl")), CancellationToken.None);
            
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var table = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='table3']");
            var countries = new List<Country>();
            foreach (var row in table.SelectSingleNode("tbody").SelectNodes("tr")) {
                var cells = row.SelectNodes("td");
                var country = new Country {
                    Name = cells[0].InnerText.Trim(),
                    TotalCases = GetIntValue(cells[1]),
                    NewCases = GetIntValue(cells[2]),
                    TotalDeaths = GetIntValue(cells[3]),
                    NewDeaths = GetIntValue(cells[4]),
                    TotalRecovered = GetIntValue(cells[5]),
                    Serious = GetIntValue(cells[6])
                };
                countries.Add(country);
            }

            int GetIntValue(HtmlNode cell) {
                var value = cell.InnerText.Trim();
                if (string.IsNullOrEmpty(value))
                    return 0;

                return Int32.Parse(value.Replace(",", ""));
            }

            return countries;
        }
    }
}
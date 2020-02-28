using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;

namespace Notifier
{
    public static class CountryFunction
    {
        [FunctionName("GetCountriesFromDB")]
        public static async Task<IDictionary<string, Country>> GetCountriesFromDB([ActivityTrigger] IDurableActivityContext context, ILogger log) {
            var repository = new Repository();
            var countries = await repository.GetCountriesAsync();
            var countriesSet = new Dictionary<string, Country>();
            foreach (var countryItem in countries) {
                if (countriesSet.ContainsKey(countryItem.Name))
                    continue;

                countriesSet.Add(countryItem.Name, countryItem);
            }
            return countriesSet;
        }

        [FunctionName("UpdateCountry")]
        public static async Task UpdateCountry([ActivityTrigger] IDurableActivityContext context, ILogger log) {
            var country = context.GetInput<Country>();
            var repository = new Repository();
            await repository.UpdateAsync(country);
        }

        [FunctionName("CreateCountry")]
        public static async Task CreateCountry([ActivityTrigger] IDurableActivityContext context, ILogger log) {
            var country = context.GetInput<Country>();
            var repository = new Repository();
            await repository.CreateAsync(country);
        }
    }
}
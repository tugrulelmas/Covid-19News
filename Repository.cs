using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notifier
{
    internal class Repository
    {
        private readonly IMongoCollection<Country> countries;

        public Repository() {
            var connectionString = Environment.GetEnvironmentVariable("MongoDBConnectionString");
            var client = new MongoClient(connectionString);            
            var database = client.GetDatabase("WuhanVirus");
            countries = database.GetCollection<Country>("Countries");
        }

        public async Task<IEnumerable<Country>> GetCountriesAsync() {
            var result = await countries.Find(x => true).ToListAsync();
            return result;
        }

        public async Task<Country> CreateAsync(Country country) {
            await countries.InsertOneAsync(country);
            return country;
        }

        public async Task UpdateAsync(Country country) =>
            await countries.ReplaceOneAsync(c => c.Name == country.Name, country);
    }
}

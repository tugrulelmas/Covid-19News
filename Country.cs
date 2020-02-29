using MongoDB.Bson.Serialization.Attributes;

namespace Notifier
{
    public class Country
    {
        [BsonId]
        public string Name { get; set; }

        public int TotalCases { get; set; }

        public int NewCases { get; set; }

        public int ActiveCases { get; set; }

        public int TotalDeaths { get; set; }

        public int NewDeaths { get; set; }

        public int TotalRecovered { get; set; }

        public int Serious { get; set; }
    }
}
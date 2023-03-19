using CsvHelper.Configuration.Attributes;
using MongoDB.Bson;

namespace DataCompare.Api.Models
{
    public class MongoRecord
    {
        public ObjectId Id { get; set; }
        [Name("card_serial")]
        public string card_serial { get; set; }
    }
}

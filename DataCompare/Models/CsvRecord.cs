using CsvHelper.Configuration.Attributes;

namespace DataCompare.Api.Models
{
    public class CsvRecord
    {
        [Name("card_serial")]
        public string card_serial { get; set; }
    }
}

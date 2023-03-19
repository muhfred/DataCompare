using CsvHelper;
using CsvHelper.Configuration;
using DataCompare.Api.Models;
using DnsClient.Protocol;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataCompare.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CsvController : ControllerBase
    {
        private readonly IMongoCollection<MongoRecord> _mongoCollection;

        public CsvController(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("test");
            _mongoCollection = database.GetCollection<MongoRecord>("testCollection");
        }

        [HttpPost("uploadCsv")]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest();

            var filePath = Path.GetTempFileName();

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var csvRecords = ReadCsv(filePath);

            var mongoRecords = await _mongoCollection.Find(x => true).ToListAsync();

            var diff = csvRecords.Where(csvRecord => !mongoRecords.Any(mongoRecord => mongoRecord.card_serial == csvRecord.card_serial)).ToList();

            return Ok(diff);
        }
        private static List<CsvRecord> ReadCsv(string filePath)
        {

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ",", HasHeaderRecord = true }))
            {
                var records = csv.GetRecords<CsvRecord>().ToList();
                return records;
            }
            
        }
    }
}
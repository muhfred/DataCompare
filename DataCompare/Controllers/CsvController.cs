using CsvHelper;
using CsvHelper.Configuration;
using DataCompare.Api.Models;
using DnsClient.Protocol;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;
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

        [HttpPost("compareCsv")]
        public async Task<ActionResult<CsvComparerResult>> UploadCsv(IFormFile file1, IFormFile file2)
        {
            if (file1 == null || file1.Length == 0)
                return BadRequest();

            if (file2 == null || file2.Length == 0)
                return BadRequest();

            var filePath1 = Path.GetTempFileName();

            using (var stream = new FileStream(filePath1, FileMode.Create))
            {
                await file1.CopyToAsync(stream);
            }
            var filePath2 = Path.GetTempFileName();

            using (var stream = new FileStream(filePath2, FileMode.Create))
            {
                await file2.CopyToAsync(stream);
            }
            CsvComparerService comparerService = new();

           var result = comparerService.Compare(filePath1, filePath2);

            return Ok(result);
        }


    }
    public class CsvComparer : IEqualityComparer<CsvRecord>
    {
        public bool Equals(CsvRecord x, CsvRecord y)
        {
            return x.card_serial == y.card_serial;
            // add additional fields as needed
        }

        public int GetHashCode(CsvRecord obj)
        {
            return obj.card_serial.GetHashCode() ^ obj.card_serial.GetHashCode();
            // combine additional fields as needed
        }
    }

    public class CsvComparerResult
    {
        public List<CsvRecord> onlyInFile1 { get; set; }
        public List<CsvRecord> onlyInFile2 { get; set; }
    }

    public class CsvComparerService
    {
        public CsvComparerResult Compare(string filePath1, string filePath2)
        {
            var records1 = ReadCsv(filePath1);
            var records2 = ReadCsv(filePath2);

            var comparer = new CsvComparer();

            var onlyInFile1 = records1.Except(records2, comparer).ToList();
            var onlyInFile2 = records2.Except(records1, comparer).ToList();

            return new CsvComparerResult
            {
                onlyInFile1 = onlyInFile1,
                onlyInFile2 = onlyInFile2
            };

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
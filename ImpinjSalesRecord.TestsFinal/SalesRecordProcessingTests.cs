using CsvHelper;
using CsvHelper.Configuration;
using impinj.Helper;
using impinj.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Formats.Asn1;
using System.Globalization;
using System.Net.Http.Json;
using Xunit;

namespace impinjSalesRecord.Test
{
    public class SalesRecordProcessingTests
    {
        private readonly WebApplicationFactory<Program> _factory;

        public SalesRecordProcessingTests()
        {
            _factory = new WebApplicationFactory<Program>();
        }

        /// <summary>
        /// Unit test to test Helper function
        /// </summary>
        [Fact]
        public void DateFunction_ReturnsCorrectDateTime()
        {
            // Arrange
            int year = 2024;
            int month = 9;
            int day = 12;

            // Act
            DateTime result = HelperFunctions.DateFunction(year, month, day);

            // Assert
            Assert.Equal(new DateTime(2024, 9, 12), result);
        }

        /// <summary>
        /// Test getting record information from file
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetRecordsInformation_ReturnsCorrectResult()
        {
            var client = _factory.CreateClient();
            var test = Directory.GetParent(Directory.GetCurrentDirectory()).ToString();
            string newPath = Path.GetFullPath(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).ToString(), @"..\..\test.csv"));
            var response = await client.GetAsync($"/GetRecordsInformation?csvFilePath={newPath}");

            var result = await response.Content.ReadFromJsonAsync<SalesRecordResult>();

            // Assert
            Assert.Equal("$97.44", result.MedianUnitCost);
            Assert.Equal("Middle East and North Africa", result.MostCommonRegion);
            Assert.Equal(0, result.DaysBetweenFirstAndLastOrder);
            Assert.Equal("$142,509.72", result.TotalRevenue);
        }

        /// <summary>
        /// Test correct outputs with in-memory string
        /// </summary>
        [Fact]
        public void SalesRecordMap_MapsCorrectly()
        {
            // Arrange
            var csv = @"Region,Country,Item Type,Sales Channel,Order Priority,Order Date,Order ID,Ship Date,Units Sold,Unit Price,Unit Cost,Total Revenue,Total Cost,Total Profit" + "\n" +
                        "Middle East and North Africa,Azerbaijan,Snacks,Online,C,10/8/2014,535113847,10/23/2014,934,152.58,97.44,142509.72,91008.96,51500.76";

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ","
            };

            using (var reader = new StringReader(csv))
            using (var csvReader = new CsvReader(reader, config))
            {
                csvReader.Context.RegisterClassMap<SalesRecordMap>();

                // Act
                var record = csvReader.GetRecords<SalesRecord>().First();

                // Assert
                Assert.Equal(new DateTime(2014, 10, 8), record.OrderDate);
                Assert.Equal(new DateTime(2014, 10, 23), record.ShipDate);
                Assert.Equal("Middle East and North Africa", record.Region);
                Assert.Equal(142509.72m, record.TotalRevenue);
            }
        }
    }
}
using CsvHelper;
using CsvHelper.Configuration;
using impinj.Helper;
using impinj.Models;
using System.Formats.Asn1;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Create a minimal endpoint to get the records information
app.MapGet("/GetRecordsInformation", async (string csvFilePath) =>
{
    // Create a configuration for CsvHelper
    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        Delimiter = ","
    };

    using (var reader = new StreamReader(csvFilePath))
    using (var csv = new CsvReader(reader, config))
    {
        csv.Context.RegisterClassMap<SalesRecordMap>();
        // Read the records from the CSV
        var records = csv.GetRecords<SalesRecord>().ToList();

        // Process the records as needed
        foreach (var record in records)
        {
            // Use the Callback handler to process dates
            Callback handler = HelperFunctions.DateFunction;
            record.OrderDate = handler(record.OrderDate.Year, record.OrderDate.Month, record.OrderDate.Day);
            record.ShipDate = handler(record.ShipDate.Year, record.ShipDate.Month, record.ShipDate.Day);
        }

        decimal totalRevenue = records.Sum(r => r.TotalRevenue);

        // Calculate the median Unit Cost
        var sortedUnitCosts = records.Select(r => r.UnitCost).OrderBy(cost => cost).ToList();
        int count = sortedUnitCosts.Count;
        decimal medianUnitCost = (count % 2 == 0) ?
                                 (sortedUnitCosts[count / 2 - 1] + sortedUnitCosts[count / 2]) / 2 :
                                 sortedUnitCosts[count / 2];

        // Find the most common region
        string mostCommonRegion = records.GroupBy(r => r.Region)
                                         .OrderByDescending(g => g.Count())
                                         .First().Key;

        // Find the first and last order dates and calculate the days between them
        DateTime firstOrderDate = records.Min(r => r.OrderDate);
        DateTime lastOrderDate = records.Max(r => r.OrderDate);
        int daysBetweenFirstAndLastOrder = (lastOrderDate - firstOrderDate).Days;

        return new SalesRecordResult()
        {
            MedianUnitCost = medianUnitCost.ToString("C"),
            MostCommonRegion = mostCommonRegion,
            DaysBetweenFirstAndLastOrder = daysBetweenFirstAndLastOrder,
            TotalRevenue = totalRevenue.ToString("C")
        };
    }
})
.WithName("GetRecordsInformation")
.WithOpenApi();

app.Run();

delegate DateTime Callback(int a, int b, int c);

public partial class Program { }

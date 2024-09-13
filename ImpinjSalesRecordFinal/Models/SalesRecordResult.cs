namespace impinj.Models
{
    public class SalesRecordResult
    {
        public string TotalRevenue { get; set; }
        public string MedianUnitCost { get; set; }
        public string MostCommonRegion { get; set; }
        public int DaysBetweenFirstAndLastOrder { get; set; }
    }
}

namespace jira_leadtime_calculator.Settings
{
    public class PublicHolidaySettings
    {
        public Dictionary<string, CountrySpecificPublicHolidaySettings> PublicHolidays { get; set; }
    }

    public class CountrySpecificPublicHolidaySettings
    {
        public string Country { get; set; }
        public List<DateTime> PublicHolidays { get; set; }
    }
}

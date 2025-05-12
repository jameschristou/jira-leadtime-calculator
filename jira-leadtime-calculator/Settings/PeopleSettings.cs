namespace jira_leadtime_calculator.Settings
{
    public class PeopleSettings : List<Person>
    {
    }

    public class Person
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public List<DateTime> LeaveDays { get; set; }
    }
}

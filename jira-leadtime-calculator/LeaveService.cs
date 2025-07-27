using jira_leadtime_calculator.Settings;

namespace jira_leadtime_calculator
{
    public interface ILeaveService
    {
        List<DateTime> GetLeaveDates(string name);
    }

    public class LeaveService : ILeaveService
    {
        private readonly PeopleSettings _peopleSettings;
        private readonly PublicHolidaySettings _publicHolidaySettings;

        public LeaveService(PeopleSettings peopleSettings, PublicHolidaySettings publicHolidaySettings)
        {
            _peopleSettings = peopleSettings;
            _publicHolidaySettings = publicHolidaySettings;
        }

        public List<DateTime> GetLeaveDates(string name)
        {
            var personSettings = _peopleSettings.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (personSettings == null || string.IsNullOrEmpty(personSettings.Country))
            {
                Console.WriteLine($"{name}'s country not found in settings.");
                return new List<DateTime>();
            }

            if(!_publicHolidaySettings.PublicHolidays.TryGetValue(personSettings.Country, out var countryPublicHolidays)) 
            {
                Console.WriteLine($"{personSettings.Country} country not found in settings.");
                return new List<DateTime>(); 
            }

            var leaveDates = countryPublicHolidays;

            if(personSettings.LeaveDays != null)
            {
                // need to also apply the individual's leave dates
                leaveDates.AddRange(personSettings.LeaveDays);
            }
            
            return leaveDates
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }
    }
}

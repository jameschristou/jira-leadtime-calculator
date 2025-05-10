namespace jira_leadtime_calculator
{
    public class LeadTimeCalculator
    {
        public int Calculate(IssueData issueData)
        {
            if (!issueData.Resolved.HasValue) return 0;

            int totalDays = (issueData.Resolved.Value - issueData.DateMovedToInProgress.Value).Days + 1; // Include the end date
            int leadTimeInDays = 0;

            for (int i = 0; i < totalDays; i++)
            {
                var currentDate = issueData.DateMovedToInProgress.Value.AddDays(i);
                if (IsWorkingDay(currentDate))
                {
                    leadTimeInDays++;
                }
            }
            return leadTimeInDays;
        }

        private bool IsWorkingDay(DateTime date)
        {
            // Check if the date is a weekend (Saturday or Sunday)
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }

            return true;
        }
    }
}

namespace jira_leadtime_calculator
{
    public interface ILeadTimeCalculator
    {
        int Calculate(LeadTimeData issueData);
    }

    public class LeadTimeCalculator : ILeadTimeCalculator
    {
        private readonly ILeaveService _leaveService;

        public LeadTimeCalculator(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        public int Calculate(LeadTimeData issueData)
        {
            if (!issueData.DateResolved.HasValue) return 0;

            var issueStartedDate = GetIssueStartedDate(issueData);

            int totalDays = (issueData.DateResolved.Value - issueStartedDate).Days + 1; // Include the end date
            int leadTimeInDays = 0;

            var leaveDates = _leaveService.GetLeaveDates(issueData.Assignee);

            for (int i = 0; i < totalDays; i++)
            {
                var currentDate = issueStartedDate.AddDays(i);
                if (IsWorkingDay(currentDate, leaveDates))
                {
                    leadTimeInDays++;
                }
            }
            return leadTimeInDays;
        }

        private bool IsWorkingDay(DateTime date, List<DateTime> leaveDates)
        {
            // Check if the date is a weekend (Saturday or Sunday)
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }

            // Check if the date is a public holiday
            return !leaveDates.Contains(date.Date);
        }

        private DateTime GetIssueStartedDate(LeadTimeData issueData)
        {
            if (issueData.DateMovedToInProgress.HasValue)
            {
                return issueData.DateMovedToInProgress.Value;
            }
            else if (issueData.DateMovedToInReview.HasValue)
            {
                return issueData.DateMovedToInReview.Value;
            }
            else if (issueData.DateMovedToReadyToTest.HasValue)
            {
                return issueData.DateMovedToReadyToTest.Value;
            }
            else if (issueData.DateMovedToInTest.HasValue)
            {
                return issueData.DateMovedToInTest.Value;
            }
            else
            {
                throw new Exception($"Issue {issueData.JiraIssueKey} does not have a start date.");
            }
        }
    }
}

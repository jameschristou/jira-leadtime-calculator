namespace jira_leadtime_calculator
{
    public class LeadTimeData
    {
        public string JiraIssueKey { get; set; }
        public string Summary { get; set; }
        public string CurrentStatus { get; set; }
        public string Assignee { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateMovedToInProgress { get; set; } // In Progress
        public DateTime? DateMovedToInReview { get; set; }
        public DateTime? DateMovedToReadyToTest { get; set; } // Ready to Test
        public DateTime? DateMovedToInTest { get; set; } // In Test, Test Feedback
        public DateTime? DateMovedToReadyToRelease { get; set; } // Ready to Release
        public DateTime? DateResolved { get; set; }
        public int TotalLeadTimeDays { get; set; }
    }
}

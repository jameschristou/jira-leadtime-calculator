using jira_leadtime_calculator.JiraApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jira_leadtime_calculator
{
    public class IssueData
    {
        public string IssueKey { get; set; }
        public string Summary { get; set; }
        public string Status { get; set; }
        public DateTime? DateMovedToInProgress { get; set; } // In Progress
        public DateTime? DateMovedToInReview { get; set; }
        public DateTime? DateMovedToReadyToTest { get; set; } // Ready to Test
        public DateTime? DateMovedToInTest { get; set; } // In Test, Test Feedback
        public DateTime? DateMovedToReadyToRelease { get; set; } // Ready to Release
        public DateTime Created { get; set; }
        public DateTime Resolved { get; set; }
    }

    /// <summary>
    /// Things to remember: status can be skipped e.g. going from In Progress to Ready to Test
    /// Probably the way to approach it is to look for the first log where the status changes to the given status we are looking for
    /// </summary>

    public class JiraService
    {
        private readonly IJiraApiClient _apiClient;

        public JiraService(IJiraApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<IssueData>> GetIssues(string jql)
        {
            var response = await _apiClient.SearchIssues(jql);

            return response.issues.Select(x =>
            {
                if (int.TryParse(x.id, out var id))
                {
                    return new IssueData
                    {
                        IssueKey = x.key,
                        Summary = x.fields.summary,
                        Status = x.fields.status.name,
                    };
                }

                throw new Exception($"Unable to retrieve jira");
            }).ToList();

            // for each of the issues returned we need to get their changelog data. This needs to be done individually for each jira issue

            // an improvement would be to read the google sheet first. For anything which is marked as done and already on the sheet, we don't need to read it again
        }
    }
}

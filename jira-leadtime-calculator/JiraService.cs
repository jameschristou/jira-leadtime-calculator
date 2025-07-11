﻿using jira_leadtime_calculator.JiraApiClient;
using System.Collections.Generic;

namespace jira_leadtime_calculator
{
    public class IssueData
    {
        public string IssueKey { get; set; }
        public string Summary { get; set; }
        public string Status { get; set; }
        public string Assignee { get; set; }
        public DateTime? DateMovedToInProgress { get; set; } // In Progress
        public DateTime? DateMovedToInReview { get; set; }
        public DateTime? DateMovedToReadyToTest { get; set; } // Ready to Test
        public DateTime? DateMovedToInTest { get; set; } // In Test, Test Feedback
        public DateTime? DateMovedToReadyToRelease { get; set; } // Ready to Release
        public DateTime Created { get; set; }
        public DateTime? Resolved { get; set; }
    }

    public class IssueStatusChangeData
    {
        public DateTime? DateMovedToInProgress { get; set; } // In Progress
        public DateTime? DateMovedToInReview { get; set; }
        public DateTime? DateMovedToReadyToTest { get; set; } // Ready to Test
        public DateTime? DateMovedToInTest { get; set; } // In Test, Test Feedback
        public DateTime? DateMovedToReadyToRelease { get; set; } // Ready to Release
        public DateTime? DateResolved { get; set; }
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
            var pageNumber = 1;
            var pageSize = 50;

            var results = new List<IssueData>();

            while(1 == 1)
            {
                var startIndex = (pageNumber - 1) * pageSize;
                var response = await _apiClient.SearchIssues(jql, startIndex, pageSize);

                results.AddRange(response.issues.Select(x =>
                {
                    if (int.TryParse(x.id, out var id))
                    {
                        // try and get 
                        var statusChangedDate = DateTime.Parse(x.fields.statuscategorychangedate);

                        return new IssueData
                        {
                            IssueKey = x.key,
                            Summary = x.fields.summary,
                            Status = x.fields.status.name,
                            Assignee = x.fields.assignee?.displayName,
                            Created = DateTime.Parse(x.fields.created)
                        };
                    }

                    throw new Exception($"Unable to retrieve jira");
                }));

                if (pageNumber*pageSize > response.total)
                {
                    break;
                }

                pageNumber++;
            }

            return results;

            // for each of the issues returned we need to get their changelog data. This needs to be done individually for each jira issue

            // an improvement would be to read the google sheet first. For anything which is marked as done and already on the sheet, we don't need to read it again
        }

        public async Task<IssueStatusChangeData> GetIssueStatusChangeData(string issueKey)
        {
            var response = await _apiClient.GetIssueChangeLog(issueKey);

            var changeLogs = response.values.OrderBy(x => x.created).ToList();

            return new IssueStatusChangeData
            {
                DateMovedToInProgress = GetFirstStatusChangeDate("In Progress", changeLogs),
                DateMovedToInReview = GetFirstStatusChangeDate("In Review", changeLogs),
                DateMovedToReadyToTest = GetFirstStatusChangeDate("Ready to Test", changeLogs),
                DateMovedToInTest = GetFirstStatusChangeDate("In Test", changeLogs),
                DateMovedToReadyToRelease = GetFirstStatusChangeDate("Ready to Release", changeLogs),
                DateResolved = GetLastStatusChangeDate("Done", changeLogs)
            };
        }

        private DateTime? GetFirstStatusChangeDate(string status, List<IssueChangeLogDto> changeLogs)
        {
            var statusChangeItem = changeLogs.FirstOrDefault(x => x.items.Any(y => y.field == "status" && y.toString == status));

            if (statusChangeItem == null) return null;

            return DateTime.Parse(statusChangeItem.created);
        }

        private DateTime? GetLastStatusChangeDate(string status, List<IssueChangeLogDto> changeLogs)
        {
            var statusChangeItem = changeLogs.LastOrDefault(x => x.items.Any(y => y.field == "status" && y.toString == status));

            if (statusChangeItem == null) return null;

            return DateTime.Parse(statusChangeItem.created);
        }
    }
}

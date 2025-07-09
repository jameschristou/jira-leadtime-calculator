using jira_leadtime_calculator;
using jira_leadtime_calculator.Infrastructure;
using jira_leadtime_calculator.JiraApiClient;
using jira_leadtime_calculator.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using IHost host = CreateHostBuilder(args).Build();
using var scope = host.Services.CreateScope();

var services = scope.ServiceProvider;

try
{
    // 1. first get any data on the sheet
    // 2. then run the query to get issues based on the filter
    // 3. for each issue, check the data in the sheet vs latest data from jira. If the status has changed, then we need to get the latest changelog data
    // 4. update the sheet with latest data

    var jql = "parent=\"ABC-273\" ORDER BY created ASC";

    var leadTimeData = new List<LeadTimeData>();

    // Get the data from jira
    var jiraService = services.GetService<JiraService>();
    var leadTimeCalculator = services.GetService<LeadTimeCalculator>();

    var issues = await jiraService.GetIssues(jql);

    foreach(var issue in issues)
    {
        if(issue.Status == "Rejected")
        {
            Console.WriteLine($"Issue {issue.IssueKey} is rejected. Skipping.");
            continue;
        }

        var issueChangeLog = await jiraService.GetIssueStatusChangeData(issue.IssueKey);

        var issueLeadTimeData = new LeadTimeData
        {
            JiraIssueKey = issue.IssueKey,
            Summary = issue.Summary,
            CurrentStatus = issue.Status,
            Assignee = issue.Assignee,
            DateCreated = issue.Created,
            DateMovedToInProgress = issueChangeLog.DateMovedToInProgress,
            DateMovedToInReview = issueChangeLog.DateMovedToInReview,
            DateMovedToReadyToTest = issueChangeLog.DateMovedToReadyToTest,
            DateMovedToInTest = issueChangeLog.DateMovedToInTest,
            DateMovedToReadyToRelease = issueChangeLog.DateMovedToReadyToRelease,
            DateResolved = issue.Status == "Done" ? issueChangeLog.DateResolved : null
        };

        issueLeadTimeData.TotalLeadTimeDays = leadTimeCalculator.Calculate(issueLeadTimeData);

        leadTimeData.Add(issueLeadTimeData);
    }

    var sheetService = services.GetService<GoogleSheetService>();

    await sheetService.WriteIssuesToSheet(leadTimeData);

    var test = 1;
}
catch (Exception e)
{
    Console.WriteLine("$Exception occurred:{e.Message}");
}

Console.WriteLine("Finished");

IHostBuilder CreateHostBuilder(string[] strings)
{
    return Host.CreateDefaultBuilder()
        .ConfigureServices((_, services) =>
        {
            services.AddHttpClient();

            services.AddTransient<JiraService>();
            services.AddTransient<IJiraApiClient, JiraApiClient>();
            services.AddTransient<GoogleSheetService>();
            services.AddTransient<LeadTimeCalculator>();
            services.AddTransient<ILeaveService, LeaveService>();

            // settings
            var configuration = GetConfiguration();
            services.AddConfig<PublicHolidaySettings>(configuration, "PublicHolidaySettings");
            services.AddConfig<PeopleSettings>(configuration, "PeopleSettings");
        });
}

IConfigurationRoot GetConfiguration()
{
    var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", true, true);

    return config.Build();
}
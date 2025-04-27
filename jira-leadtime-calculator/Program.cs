using jira_leadtime_calculator;
using jira_leadtime_calculator.JiraApiClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using IHost host = CreateHostBuilder(args).Build();
using var scope = host.Services.CreateScope();

var services = scope.ServiceProvider;

try
{
    var jql = "parent=\"ABC-123\" ORDER BY created ASC";

    // Get the data from jira
    var jiraService = services.GetService<JiraService>();

    var issues = await jiraService.GetIssues(jql);

    var test = 1;
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
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
            services.AddTransient<GoogleSheetWriter>();
        });
}
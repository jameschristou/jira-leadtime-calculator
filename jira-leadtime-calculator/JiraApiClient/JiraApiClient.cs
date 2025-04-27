using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace jira_leadtime_calculator.JiraApiClient
{
    public interface IJiraApiClient
    {
        Task<SearchIssuesResponseDto> SearchIssues(string jql);
    }

    public class SearchIssuesResponseDto
    {
        public List<GetIssueResponseDto> issues { get; set; }
    }

    public class GetIssueResponseDto
    {
        public string id { get; set; }
        public string key { get; set; }
        public CreateIssueRequestFieldsDto fields { get; set; }
    }

    public class CreateIssueRequestFieldsDto
    {
        public JiraPropertyWithIdOrKey issuetype { get; set; }
        public string summary { get; set; }
        public string description { get; set; }
        public JiraStatus status { get; set; }
        public List<string> labels { get; set; }
        public JiraPropertyWithIdOrKey parent { get; set; }
        public JiraPropertyWithIdOrKey project { get; set; }
        public JiraUserData assignee { get; set; }
    }

    public class JiraUserData
    {
        public string accountId { get; set; }
        public string emailAddress { get; set; }
        public string displayName { get; set; }
    }

    public class JiraStatus
    {
        public string name { get; set; }
    }

    public class JiraPropertyWithIdOrKey
    {
        public string id { get; set; }
        public string key { get; set; }

        public JiraPropertyWithIdOrKey()
        {

        }

        public JiraPropertyWithIdOrKey(int id)
        {
            this.id = id == 0 ? string.Empty : id.ToString();
        }

        public JiraPropertyWithIdOrKey(string key)
        {
            this.key = key;
        }
    }

    public class JiraApiClient : IJiraApiClient
    {
        private static string _baseUrl = "";
        private readonly HttpClient _httpClient;
        private readonly string _userToken = "";
        private readonly string _userEmailAddress = "";

        public JiraApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<SearchIssuesResponseDto> SearchIssues(string jql)
        {
            if (string.IsNullOrEmpty(jql))
            {
                return new SearchIssuesResponseDto
                {
                    issues = new List<GetIssueResponseDto>()
                };
            }

            var url = $"{_baseUrl}/rest/api/2/search?jql={jql}";

            using (var response = await _httpClient.SendAsync(BuildRequest(url, HttpMethod.Get)))
            {
                if (response == null || !response.IsSuccessStatusCode)
                {
                    throw new Exception("Unable to get search results");
                }

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = JsonSerializer.Deserialize<SearchIssuesResponseDto>(await response.Content.ReadAsStringAsync());

                    if (responseContent == null)
                    {
                        throw new Exception("Unable to get search results");
                    }

                    return responseContent;
                }

                throw new Exception("Unable to get search results");
            }
        }

        private HttpRequestMessage BuildRequest(string url, HttpMethod httpMethod)
        {
            var requestMessage = new HttpRequestMessage(httpMethod, url);

            // create a personal access token with github. See instructions here
            // https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens#creating-a-personal-access-token-classic

            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(_userToken))
            {
                // need to base64 encode the string. See Atlassian docs for details
                // https://developer.atlassian.com/cloud/jira/platform/basic-auth-for-rest-apis/
                var encodedAuth = System.Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_userEmailAddress}:{_userToken}"));
                requestMessage.Headers.Add("Authorization", $"Basic {encodedAuth}");
            }

            return requestMessage;
        }
    }
}

﻿using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace jira_leadtime_calculator.JiraApiClient
{
    public interface IJiraApiClient
    {
        Task<SearchIssuesResponseDto> SearchIssues(string jql, int resultsStartIndex, int pageSize);
        Task<IssueChangeLogResponse> GetIssueChangeLog(string issueKey);
    }

    public class SearchIssuesResponseDto
    {
        public List<GetIssueResponseDto> issues { get; set; }
        public int total { get; set; } // total number of results
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
        public string statuscategorychangedate { get; set; }
        public string created { get; set; }
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

    public class IssueChangeLogResponse
    {
        public List<IssueChangeLogDto> values { get; set; }
    }

    public class IssueChangeLogDto
    {
        public string created { get; set; }
        public List<IssueChangeLogItemDto> items { get; set; }
    }

    public class IssueChangeLogItemDto
    {
        public string field { get; set; }
        public string fromString { get; set; }
        public string toString { get; set; }
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

        public async Task<SearchIssuesResponseDto> SearchIssues(string jql, int resultsStartIndex, int pageSize)
        {
            // using https://developer.atlassian.com/cloud/jira/platform/rest/v2/api-group-issue-search/#api-rest-api-2-search-get

            if (string.IsNullOrEmpty(jql))
            {
                return new SearchIssuesResponseDto
                {
                    issues = new List<GetIssueResponseDto>()
                };
            }

            var url = $"{_baseUrl}/rest/api/2/search?jql={jql}&startAt={resultsStartIndex}&maxResults={pageSize}";

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

        public async Task<IssueChangeLogResponse> GetIssueChangeLog(string issueKey)
        {
            // using https://developer.atlassian.com/cloud/jira/platform/rest/v2/api-group-issues/#api-rest-api-2-issue-issueidorkey-changelog-get

            if (string.IsNullOrEmpty(issueKey))
            {
                return new IssueChangeLogResponse();
            }

            var url = $"{_baseUrl}/rest/api/2/issue/{issueKey}/changelog";

            using (var response = await _httpClient.SendAsync(BuildRequest(url, HttpMethod.Get)))
            {
                if (response == null || !response.IsSuccessStatusCode)
                {
                    throw new Exception("Unable to get search results");
                }

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = JsonSerializer.Deserialize<IssueChangeLogResponse>(await response.Content.ReadAsStringAsync());

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

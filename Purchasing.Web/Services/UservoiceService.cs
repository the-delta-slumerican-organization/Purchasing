﻿using System.Linq;
using System.Net;
using System.Text;
using System.Web.Configuration;
using Newtonsoft.Json.Linq;
using Purchasing.Web.Helpers;
using System.Collections.Generic;

namespace Purchasing.Web.Services
{
    //const string query = "https://ucdavis.uservoice.com/api/v1/forums/126891/suggestions.json?category=31579&sort=newest";
    //const string query = "https://ucdavis.uservoice.com/api/v1/forums/126891/categories/31577.json";
    //const string query = "https://ucdavis.uservoice.com/api/v1/forums/126891/categories.json";
    //const string query = "https://ucdavis.uservoice.com/api/v1/users/24484752.json";

    public interface IUservoiceService
    {
        int GetActiveIssuesCount();
        List<JToken> GetOpenIssues();

        /// <summary>
        /// Filters a list of issues by status name (can be null)
        /// </summary>
        List<JToken> FilterIssuesByStatus(List<JToken> issues, string status);

        /// <summary>
        /// Set the status of any issue
        /// </summary>
        /// <param name="id">issue id</param>
        /// <param name="status">Must be one of the 5 status options on ucdavis.uservoice</param>
        void SetIssueStatus(int id, string status);
    }

    /// <summary>
    /// Encapsulates API calls to uservoice using OAuth
    /// </summary>
    /// <remarks>See docs for result shape and call options</remarks>
    /// <see cref="http://developer.uservoice.com/docs/api-public/"/>
    public class UservoiceService : IUservoiceService
    {
        private static readonly string ApiKey = WebConfigurationManager.AppSettings["uservoiceKey"];
        private static readonly string ApiSecret = WebConfigurationManager.AppSettings["uservoiceSecret"];
        private const string ApiUrlBase = "https://ucdavis.uservoice.com";
        private const string ForumId = "126891";
        private const string IssuesCategoryId = "31579";

        /// <summary>
        /// Returns a list of open issues, each as a json token
        /// </summary>
        public List<JToken> GetOpenIssues()
        {
            string endpoint = CreateEndpoint("/api/v1/forums/{0}/suggestions.json?category={1}&sort=newest&per_page=100");

            var result = PerformApiCall(endpoint);

            var allIssues = JObject.Parse(result);
            var openIssues = allIssues["suggestions"].Children().Where(x => x["closed_at"].Value<string>() == null).ToList();

            return openIssues;
        }

        /// <summary>
        /// Filters a list of issues by status name (can be null)
        /// </summary>
        public List<JToken> FilterIssuesByStatus(List<JToken> issues, string status)
        {
            return issues.Where(x => x["status"].Value<string>() == status).ToList();
        }

        /// <summary>
        /// Set the status of any issue
        /// </summary>
        /// <param name="id">issue id</param>
        /// <param name="status">Must be one of the 5 status options on ucdavis.uservoice</param>
        public void SetIssueStatus(int id, string status)
        {
            string endpoint = string.Format("/api/v1/forums/{0}/suggestions/{1}/respond.json", ForumId, id);

            var data = string.Format("notify=false&response[status]={0}", status);

            PerformApiCall(endpoint, "PUT", data);
        }

        public int GetActiveIssuesCount()
        {
            string endpoint = CreateEndpoint("/api/v1/forums/{0}/categories.json");

            var result = JObject.Parse(PerformApiCall(endpoint));

            var issueCategory =
                result["categories"].Children().Single(c => c["name"].Value<string>() == "Issues");

            return issueCategory["suggestions_count"].Value<int>();
        }

        /// <summary>
        /// Performs a query against the ucdavis prepurchasing uservoice using the desired endpoint and http method
        /// </summary>
        /// <param name="endpoint">/api/... </param>
        /// <param name="method">GET/POST/PUT/DELETE</param>
        /// <param name="data">Request data, ex: field1=abc&field2=def12</param>
        /// <remarks>http://developer.uservoice.com/docs/api-public/</remarks>
        /// <returns>Result string from API call</returns>
        private string PerformApiCall(string endpoint, string method = "GET", string data = null)
        {
            var query = ApiUrlBase + endpoint;

            var oauth = new Manager();
            oauth["consumer_key"] = ApiKey;
            oauth["consumer_secret"] = ApiSecret;

            var header = oauth.GenerateAuthzHeader(query, method);

            var req = (HttpWebRequest)WebRequest.Create(query);
            req.Method = method;
            req.Headers.Add("Authorization", header);

            if (data != null)
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(data);
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = byteArray.Length;
                var dataStream = req.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }

            using (var response = (HttpWebResponse) req.GetResponse())
            {
                using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Pass in a tokenized string with {0} to be replaced with forumId, {1} replaced with the issues category id
        /// </summary>
        private string CreateEndpoint(string tokenizedEndpoint)
        {
            return string.Format(tokenizedEndpoint, ForumId, IssuesCategoryId);
        }
    }
}
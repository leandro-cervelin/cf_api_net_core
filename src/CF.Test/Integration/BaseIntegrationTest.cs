using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using CF.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;

namespace CF.Test.Integration
{
    public class BaseIntegrationTest
    {
        protected WebApplicationFactory<Startup> Host;

        protected HttpClient Client;

        public BaseIntegrationTest()
        {
            if (Client != null) return;
            Client = CreateHttpClient();
        }

        public HttpRequestMessage BuildHttpRequest(HttpMethod method, string url, string content, Dictionary<string, string> headerParams = null)
        {
            var httpRequestMessage = new HttpRequestMessage(method, url);

            SetHttpRequestMessageHeaders(headerParams);
            
            if (string.IsNullOrEmpty(content)) return httpRequestMessage;
            httpRequestMessage.Content = new StringContent(content);
            httpRequestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return httpRequestMessage;
        }

        public HttpRequestMessage BuildPostHttpRequest(string url, object content, Dictionary<string, string> headerParams = null)
        {
            return BuildHttpRequest(HttpMethod.Post, url, JsonConvert.SerializeObject(content), headerParams);
        }

        public HttpRequestMessage BuildPutHttpRequest(string url, object content, Dictionary<string, string> headerParams = null)
        {
            return BuildHttpRequest(HttpMethod.Put, url, JsonConvert.SerializeObject(content), headerParams);
        }

        public HttpRequestMessage BuildDeleteHttpRequest(string url, Dictionary<string, string> headerParams = null)
        {
            return BuildHttpRequest(HttpMethod.Delete, url, null, headerParams);
        }

        public HttpRequestMessage BuildGetHttpRequest(string url, Dictionary<string, string> queryString, Dictionary<string, string> headerParams = null)
        {
            var finalUrl = queryString?.Count > 0 ? QueryHelpers.AddQueryString(url, queryString) : url;

            return BuildHttpRequest(HttpMethod.Get, finalUrl, string.Empty, headerParams);
        }

        private HttpClient CreateHttpClient()
        {
            var host = new WebHostBuilderFactory<Startup>();
            const string solutionRelativePath = "src/CF";
            Host = host.WithWebHostBuilder(builder => { builder.UseSolutionRelativeContentRoot(solutionRelativePath); });
            var client = host.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-Correlation-ID", Guid.NewGuid().ToString());
            return client;
        }

        private void SetHttpRequestMessageHeaders(Dictionary<string, string> headerParams)
        {
            if (headerParams == null || !headerParams.Any()) return;
            foreach (var (key, value) in headerParams) 
                Client.DefaultRequestHeaders.Add(key, value);
        }

        public string BuildQueryString(string url, Dictionary<string, string> queryString)
        {
            return QueryHelpers.AddQueryString(url, queryString);
        }
    }
}

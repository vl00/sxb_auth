using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Library
{
    public class HttpHelper
    {/// <summary>
     /// 发起POST同步请求
     /// 
     /// </summary>
     /// <param name="url"></param>
     /// <param name="postData"></param>
     /// <param name="contentType">application/xml、application/json、application/text、application/x-www-form-urlencoded</param>
     /// <param name="headers">填充消息头</param>        
     /// <returns></returns>
        public static string HttpPost(string url, object postData = null, string contentType = null, int timeOut = 30, Dictionary<string, string> headers = null)
        {
            return HttpPost<string>(url, postData, contentType, timeOut, headers);
        }
        public static T HttpPost<T>(string url, object postData = null, string contentType = null, int timeOut = 30, Dictionary<string, string> headers = null)
        {
            postData = postData ?? "";
            using (HttpClient client = new HttpClient())
            {
                if (headers != null)
                {
                    foreach (var header in headers)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                HttpContent httpContent;
                if (postData.GetType().Equals(typeof(Stream)))
                {
                    httpContent = new StreamContent(postData as Stream);
                }
                else
                {
                    httpContent = new StringContent(postData.ToString(), Encoding.UTF8);
                }
                if (contentType != null)
                    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                HttpResponseMessage response = client.PostAsync(url, httpContent).Result;
                return Return<T>(response);
            }
        }
        public static T HttpPost<T>(string url, Stream postData = null, string contentType = null, int timeOut = 30, Dictionary<string, string> headers = null)
        {
            using (HttpClient client = new HttpClient())
            {
                if (headers != null)
                {
                    foreach (var header in headers)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                using (HttpContent httpContent = new StreamContent(postData))
                {
                    if (contentType != null)
                        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    HttpResponseMessage response = client.PostAsync(url, httpContent).Result;
                    return Return<T>(response);
                }
            }
        }


        /// <summary>
        /// 发起POST异步请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="contentType">application/xml、application/json、application/text、application/x-www-form-urlencoded</param>
        /// <param name="headers">填充消息头</param>        
        /// <returns></returns>
        public static async Task<string> HttpPostAsync(string url, string postData = null, string contentType = null, int timeOut = 30, Dictionary<string, string> headers = null)
        {
            return await HttpPostAsync<string>(url, postData, contentType, timeOut, headers);
        }
        public static async Task<T> HttpPostAsync<T>(string url, string postData = null, string contentType = null, int timeOut = 30, Dictionary<string, string> headers = null)
        {
            postData = postData ?? "";
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, timeOut);
                if (headers != null)
                {
                    foreach (var header in headers)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                using (HttpContent httpContent = new StringContent(postData, Encoding.UTF8))
                {
                    if (contentType != null)
                        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                    HttpResponseMessage response = await client.PostAsync(url, httpContent);
                    return await ReturnAsync<T>(response);
                }
            }
        }

        /// <summary>
        /// 发起GET同步请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static string HttpGet(string url, Dictionary<string, string> headers = null)
        {
            return HttpGet<string>(url, headers);
        }
        public static T HttpGet<T>(string url, Dictionary<string, string> headers = null)
        {
            using (HttpClient client = new HttpClient())
            {
                if (headers != null)
                {
                    foreach (var header in headers)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                HttpResponseMessage response = client.GetAsync(url).Result;
                return Return<T>(response);
            }
        }
        /// <summary>
        /// 发起GET异步请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static async Task<string> HttpGetAsync(string url, Dictionary<string, string> headers = null)
        {
            return await HttpGetAsync<string>(url, headers);
        }
        public static async Task<T> HttpGetAsync<T>(string url, Dictionary<string, string> headers = null)
        {
            using (HttpClient client = new HttpClient())
            {
                if (headers != null)
                {
                    foreach (var header in headers)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                HttpResponseMessage response = await client.GetAsync(url);
                return await ReturnAsync<T>(response);
            }
        }
        public static T Return<T>(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    response.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
                    return response.Content.ReadAsAsync<T>().Result;
                }
            }
            return default;
        }
        public static async Task<T> ReturnAsync<T>(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseStr = response.Content.ReadAsStringAsync().Result.Trim();
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)responseStr;
                }
                else
                {
                    try
                    {
                        if (CommonHelper.IsJsonp(responseStr, out string jsonContent))
                        {
                            return JsonConvert.DeserializeObject<T>(jsonContent);
                        }
                        return JsonConvert.DeserializeObject<T>(responseStr);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("返回数据解析失败");
                    }
                    //response.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
                    //return await response.Content.ReadAsAsync<T>();
                }
            }
            return default;
        }
    }
}

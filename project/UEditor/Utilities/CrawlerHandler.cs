﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace UEditor.Utitlies
{
    /// <summary>
    /// Crawler 的摘要说明
    /// </summary>
    public class CrawlerHandler : Handler
    {
        private string[] Sources;
        private Crawler[] Crawlers;
        public CrawlerHandler(HttpContext context) : base(context) { }

        public override IActionResult Process
        {
            get
            {
                Sources = Request.Form["source[]"];
                if (Sources == null || Sources.Length == 0)
                {
                    return WriteJson(new
                    {
                        state = "参数错误：没有指定抓取源"
                    });
                }
                Guid.TryParse(Request.Query["contentID"], out Guid contentID);
                string contentType = Request.Query["contentType"];
                Crawlers = Sources.Select(x => new Crawler(x).Fetch(contentID, contentType)).ToArray();
                return WriteJson(new
                {
                    state = "SUCCESS",
                    list = Crawlers.Select(x => new
                    {
                        state = x.State,
                        source = x.SourceUrl,
                        url = x.ServerUrl,
                        id = x.FileID
                    })
                });
            }
        }
    }

    public class Crawler
    {
        public string SourceUrl { get; set; }
        public string CatchUrl { get; set; }
        public string ServerUrl { get; set; }
        public Guid FileID { get; set; }
        public string State { get; set; }

        //private HttpServerUtility Server { get; set; }


        public Crawler(string sourceUrl)
        {
            this.SourceUrl = sourceUrl;
            this.CatchUrl = sourceUrl.Replace("&tp=webp", "").Replace("?tp=webp", "?");
        }

        public Crawler Fetch(Guid contentID, string contentType)
        {
            if (!IsExternalIPAddress(this.SourceUrl))
            {
                State = "INVALID_URL";
                return this;
            }
            var request = HttpWebRequest.Create(this.CatchUrl) as HttpWebRequest;
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    State = "Url returns " + response.StatusCode + ", " + response.StatusDescription;
                    return this;
                }
                if (response.ContentType.IndexOf("image") == -1)
                {
                    State = "Url is not an image";
                    return this;
                }
                ServerUrl = PathFormatter.Format(Path.GetFileName(this.CatchUrl), Config.GetString("catcherPathFormat"), contentType, out Guid fileID, contentID);
                FileID = fileID;
                //var savePath = Server.MapPath(ServerUrl);
                //if (!Directory.Exists(Path.GetDirectoryName(savePath)))
                //{
                //    Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                //}
                try
                {
                    var stream = response.GetResponseStream();
                    var reader = new BinaryReader(stream);
                    byte[] bytes;
                    using (var ms = new MemoryStream())
                    {
                        byte[] buffer = new byte[4096];
                        int count;
                        while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            ms.Write(buffer, 0, count);
                        }
                        bytes = ms.ToArray();
                    }
                    if (string.IsNullOrEmpty(Path.GetExtension(ServerUrl)))
                    {
                        Library.TransportFile(bytes, ServerUrl.Replace(contentType + "/",""), out string ext, contentType, null);
                        ServerUrl += ext;
                    }
                    else
                    {
                        Library.TransportFile(bytes, ServerUrl.Replace(contentType + "/", ""), "", contentType, null);
                    }
                    //File.WriteAllBytes(savePath, bytes);
                    State = "SUCCESS";
                }
                catch (Exception e)
                {
                    State = "抓取错误：" + e.Message;
                }
                return this;
            }
        }

        private bool IsExternalIPAddress(string url)
        {
            var uri = new Uri(url);
            switch (uri.HostNameType)
            {
                case UriHostNameType.Dns:
                    var ipHostEntry = Dns.GetHostEntry(uri.DnsSafeHost);
                    foreach (IPAddress ipAddress in ipHostEntry.AddressList)
                    {
                        byte[] ipBytes = ipAddress.GetAddressBytes();
                        if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            if (!IsPrivateIP(ipAddress))
                            {
                                return true;
                            }
                        }
                    }
                    break;

                case UriHostNameType.IPv4:
                    return !IsPrivateIP(IPAddress.Parse(uri.DnsSafeHost));
            }
            return false;
        }

        private bool IsPrivateIP(IPAddress myIPAddress)
        {
            if (IPAddress.IsLoopback(myIPAddress)) return true;
            if (myIPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                byte[] ipBytes = myIPAddress.GetAddressBytes();
                // 10.0.0.0/24 
                if (ipBytes[0] == 10)
                {
                    return true;
                }
                // 172.16.0.0/16
                else if (ipBytes[0] == 172 && ipBytes[1] == 16)
                {
                    return true;
                }
                // 192.168.0.0/16
                else if (ipBytes[0] == 192 && ipBytes[1] == 168)
                {
                    return true;
                }
                // 169.254.0.0/16
                else if (ipBytes[0] == 169 && ipBytes[1] == 254)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
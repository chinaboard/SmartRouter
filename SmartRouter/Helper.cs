using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace SmartRouter
{
    public static class Helper
    {
        public static byte[] GetBytes(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string ConcatUrl(this string str, params string[] values)
        {
            if (string.IsNullOrWhiteSpace(str))
                return string.Empty;
            return string.Concat(str, string.Concat(values));
        }

        public static string Login(string url, byte[] postData, int timeout = 5000)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);

            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Method = "Post";
            httpWebRequest.Timeout = timeout;

            using (Stream stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(postData, 0, postData.Length);
                stream.Close();
            }
            using (HttpWebResponse webResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (var sr = new StreamReader(webResponse.GetResponseStream()))
                {
                    Console.WriteLine("{0}\t{1}", webResponse.StatusDescription, url);
                    return webResponse.Headers["Set-Cookie"];
                }
            }
        }

        private static byte[] BuildMultipartPostData(string Boundary, Dictionary<string, string> HttpPostData)
        {
            StringBuilder sb = new StringBuilder();

            if (HttpPostData != null && HttpPostData.Count > 0)
            {
                foreach (KeyValuePair<string, string> HttpPostDataItem in HttpPostData)
                {
                    sb.AppendLine("--" + Boundary);
                    sb.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", HttpPostDataItem.Key));
                    sb.Append(Environment.NewLine);
                    sb.AppendLine(HttpPostDataItem.Value);
                }
            }
            sb.Append("--" + Boundary + "--");

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(Encoding.UTF8.GetBytes(sb.ToString()));
                    ms.Flush();
                    ms.Position = 0;
                    return ms.ToArray();
                }
            }
        }

        public static string ChangeWANConfig(string url, string cookie, string user, string pwd)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            string boundary = "----" + "chinaboard_Pocket";
            Dictionary<string, string> HttpPostData = new Dictionary<string, string>();
            HttpPostData["cbi.submit"] = "1";
            HttpPostData["tab.network.wan"] = "general";
            HttpPostData["cbid.network.wan._fwzone"] = "wan";
            HttpPostData["cbid.network.wan._fwzone.newzone"] = string.Empty;
            HttpPostData["cbi.cbe.network.wan.ifname_single"] = "1";
            HttpPostData["cbid.network.wan.ifname_single"] = "eth1";
            HttpPostData["cbid.network.wan.proto"] = "pppoe";
            HttpPostData["cbid.network.wan.username"] = user;
            HttpPostData["cbid.network.wan.password"] = pwd;
            HttpPostData["cbid.network.wan.ac"] = string.Empty;
            HttpPostData["cbid.network.wan.service"] = string.Empty;
            HttpPostData["cbi.cbe.network.wan.auto"] = "1";
            HttpPostData["cbi.cbe.network.wan.defaultroute"] = "1";
            HttpPostData["cbid.network.wan.defaultroute"] = "1";
            HttpPostData["cbid.network.wan.metric"] = string.Empty;
            HttpPostData["cbi.cbe.network.wan.peerdns"] = "1";
            HttpPostData["cbid.network.wan.peerdns"] = "1";
            HttpPostData["cbid.network.wan._keepalive_failure"] = string.Empty;
            HttpPostData["cbid.network.wan._keepalive_interval"] = string.Empty;
            HttpPostData["cbid.network.wan.demand"] = string.Empty;
            HttpPostData["cbid.network.wan.mtu"] = string.Empty;
            HttpPostData["cbi.apply"] = "保存&应用";
            try
            {
                request.Method = "POST";
                request.Headers["Cache-Control"] = "no-cache";
                request.Headers["Pragma"] = "no-cache";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.Headers["Accept-Encoding"] = "gzip,deflate";
                request.Headers["Accept-Language"] = "zh-CN";
                request.UserAgent = "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Maxthon/4.0 Chrome/30.0.1599.101 Safari/537.36";
                request.Headers["Cookie"] = cookie;
                request.Headers["DNT"] = "1";
                request.ContentType = "multipart/form-data; boundary=" + boundary;


                request.Referer = url;
                byte[] multipartPostData = BuildMultipartPostData(boundary, HttpPostData);

                using (BinaryWriter bw = new BinaryWriter(request.GetRequestStream()))
                {
                    bw.Write(multipartPostData);
                    using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
                    {
                        using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                        {
                            Console.WriteLine("{0}\t{1}", webResponse.StatusDescription, url);
                            string responseData = sr.ReadToEnd();
                            return responseData;
                        }
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string Get(string url, string cookie)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers["Cookie"] = cookie;
            HttpWebResponse webResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (Stream stream = webResponse.GetResponseStream())
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    Console.WriteLine("{0}\t{1}", webResponse.StatusDescription, url);
                    return sr.ReadToEnd();
                }
            }
        }
    }
}

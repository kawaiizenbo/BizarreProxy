using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace BizarreProxy
{
    class Program
    {
        public static HttpListener listener;
        public static string baseUrl = "localhost";
        public static string port = "8000";
        public static int requestCount = 0;
        public static string pageData = "<!DOCTYPE html><html><head><title>BizarreProxy</title></head><body style=\"font-family: sans-serif;\"><h1>BizarreProxy</h1><form method=\"post\" action=\"bzp-proxy\"><label for=\"url\">Where would you like to go today?</label><input type=\"text\" name=\"url\"><br><input type=\"submit\" value=\"Go\"></form><hr><form method=\"post\" action=\"bzp-shutdown\"><input type=\"submit\" value=\"Emergency Shutdown\"></form></body></html>";

        public static async Task HandleIncomingConnections()
        {
            WebClient webClient;
            webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; Trident/4.0)");
            bool runServer = true;
            string targetURL = $"http://{baseUrl}:{port}";

            while (runServer)
            {
                HttpListenerContext ctx = await listener.GetContextAsync();
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine();

                if (req.Url.AbsolutePath == "/bzp-reset")
                {
                    pageData = "<!DOCTYPE html><html><head><title>BizarreProxy</title></head><body style=\"font-family: sans-serif;\"><h1>BizarreProxy</h1><form method=\"post\" action=\"bzp-proxy\"><label for=\"url\">Where would you like to go today?</label><input type=\"text\" name=\"url\"><br><input type=\"submit\" value=\"Go\"></form><hr><form method=\"post\" action=\"bzp-shutdown\"><input type=\"submit\" value=\"Emergency Shutdown\"></form></body></html>";
                }

                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/bzp-shutdown"))
                {
                    Console.WriteLine("Shutdown requested");
                    runServer = false;
                }

                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/bzp-proxy"))
                {
                    targetURL = GetRequestPostData(req).Split('=')[1].Replace("%3A", ":").Replace("%2F", "/").Replace("%3F", "?").Replace("%23", "#").Replace("%26", "&");
                    Console.WriteLine("TARGET URL: " + targetURL);
                    try
                    {
                        pageData = webClient.DownloadString(targetURL);
                    }
                    catch(Exception)
                    {
                        pageData = "Failed to do that lmao.";
                    }
                }

                if (req.Url.AbsolutePath == "/favicon.ico")
                {
                    byte[] _data = { 0, 0, 1, 0, 1, 0, 16, 16, 16, 0, 1, 0, 4, 0, 40, 1, 0, 0, 22, 0, 0, 0, 40, 0, 0, 0, 16, 0, 0, 0, 32, 0, 0, 0, 1, 0, 4, 0, 0, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 132, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 17, 17, 16, 1, 17, 17, 16, 1, 17, 17, 16, 1, 17, 17, 16, 1, 16, 0, 17, 1, 16, 0, 0, 1, 16, 0, 17, 1, 16, 0, 0, 1, 16, 0, 17, 0, 17, 0, 0, 1, 16, 0, 17, 0, 17, 0, 0, 1, 17, 17, 16, 0, 1, 16, 0, 1, 17, 17, 16, 0, 1, 16, 0, 1, 16, 0, 17, 0, 0, 17, 0, 1, 16, 0, 17, 0, 0, 17, 0, 1, 16, 0, 17, 0, 0, 1, 16, 1, 16, 0, 17, 0, 0, 1, 16, 1, 17, 17, 16, 1, 17, 17, 16, 1, 17, 17, 16, 1, 17, 17, 16, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 0, 0, 129, 129, 0, 0, 129, 129, 0, 0, 156, 159, 0, 0, 156, 159, 0, 0, 156, 207, 0, 0, 156, 207, 0, 0, 129, 231, 0, 0, 129, 231, 0, 0, 156, 243, 0, 0, 156, 243, 0, 0, 156, 249, 0, 0, 156, 249, 0, 0, 129, 129, 0, 0, 129, 129, 0, 0, 255, 255, 0, 0, };
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = _data.LongLength;
                    await resp.OutputStream.WriteAsync(_data, 0, _data.Length);
                    resp.Close();
                    continue;
                }

                if (req.Url.AbsolutePath != "/bzp-shutdown" && req.Url.AbsolutePath != "/bzp-proxy" && req.Url.AbsolutePath != "/" && req.Url.AbsolutePath != "/bzp-reset")
                {
                    byte[] _data = { 0 };
                    try
                    {
                        _data = webClient.DownloadData(targetURL + req.Url.AbsolutePath);
                    }
                    catch (Exception){}
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = _data.LongLength;
                    await resp.OutputStream.WriteAsync(_data, 0, _data.Length);
                    resp.Close();
                    continue;
                }

                byte[] data = Encoding.UTF8.GetBytes(pageData);
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }


        public static void Main(string[] args)
        {
            string url = $"http://{baseUrl}:{port}/";
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();
            listener.Close();
        }

        public static string GetRequestPostData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return null;
            }
            using (Stream body = request.InputStream)
            {
                using (var reader = new StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}

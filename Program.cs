using System;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace HttpServer
{
    class HttpServer
    {
        public static HttpListener listener;
        public static string url = "http://localhost:8008/";
        public static int pageViews = 0, requestCount = 0;
        public static string pageData =
            "<!DOCTYPE>" +
            "<html>" +
            "  <head>" +
            "    <title>I'm C# HTTP server</title>" +
            "  </head>" +
            "  <body>" +
            "    <p>Page Views: {0}</p>" +
            "    <form method=\"post\" action=\"shutdown\">" +
            "      <input type=\"submit\" value=\"Shutdown\" {1}>" +
            "    </form>" +
            "    <form method=\"post\" action=\"redirect\">" +
            "      <input type=\"submit\" value=\"Redirect\" {1}>" +
            "    </form>" +
            "  </body>" +
            "</html>";


        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            //HANDLE USER REQUESTS WHILE SERVER STATE IS SET AS ACTIVE
            while (runServer)
            {
                //WAIT FOR REQUEST FROM USER
                HttpListenerContext ctx = await listener.GetContextAsync();

                //GET REQUEST AND RESPONSE FROM CURRENT CONTEXT
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                //LOG INFO ABOUT REQUEST
                Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine();

                //IF SUTDOWN BUTTON IS PRESSED
                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/shutdown"))
                {
                    Console.WriteLine($"Shutdown requested");
                    runServer = false;
                }

                //IF REDIRECT BUTTON IS PRESSED
                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/redirect"))
                {
                    Console.WriteLine("User redirected");
                    resp.Redirect("https://www.llu.lv/");
                }

                //DO NOT COUNT FAVICON REQUESTS
                if (req.Url.AbsolutePath != "/favicon.ico")
                    pageViews += 1;

                //WRITE RESPONSE INFO
                string disableSubmit = !runServer ? "disabled" : "";
                byte[] data = Encoding.UTF8.GetBytes(String.Format(pageData, pageViews, disableSubmit));
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                //WRITE OUT RESPONSE ASYNC
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }

        public static void Main(string[] args)
        {
            //START HTTP SERVER AND START LISTENING FOR CONNECTIONS
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            //HANDLE REQUESTS
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            //CLOSE LISTENER STREAM
            listener.Close();
        }
    }
}
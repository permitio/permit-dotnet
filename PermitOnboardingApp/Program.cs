using System;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace PermitOnboardingApp
{
    class HttpServer
    {
        public static HttpListener listener;
        public static string url = "http://localhost:4000/";
        public static string pageData =
            "<!DOCTYPE>"
            + "<html>"
            + "  <head>"
            + "    <title>Permit fullstack authorization example</title>"
            + "  </head>"
            + "  <body>"
            + "    <p>User {0} is {1} to {2} {3}</p>"
            + "  </body>"
            + "</html>";

        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();
                HttpListenerResponse resp = ctx.Response;

                string testToken = "PJUKkuwiJkKxbIoC4o4cguWxB_2gX6MyATYKc2OCM";
                string testUser = "tester@test.com";

                string testAction = "delete";
                string testResource = "testResource";
                PermitSDK.Permit permitClient = new PermitSDK.Permit(
                    testToken,
                    "http://localhost:7000",
                    "default",
                    true
                );
                bool isUserPermitted = await permitClient.Check(testUser, testAction, testResource);
                string isPermittedText = isUserPermitted ? "Permitted" : "NOT Permitted";
                byte[] data = Encoding.UTF8.GetBytes(
                    String.Format(pageData, testUser, isPermittedText, testAction, testResource)
                );
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }

        public static void Main(string[] args)
        {
            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            this.logger.LogError("Listening for connections on {0}", url);

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        }
    }
}

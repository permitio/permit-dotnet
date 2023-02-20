using System;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using PermitSDK;

namespace PermitOnboardingApp
{
    class HttpServer
    {
        public static HttpListener listener;
        public static string url = "http://localhost:4000/";
        public static string pageData = "User {0} is {1} to {2} {3}";

        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();
                HttpListenerResponse resp = ctx.Response;

                string testToken = "";
                string testUserId = "tester@test.com";
                PermitSDK.Permit permit = new PermitSDK.Permit(
                    testToken,
                    "http://localhost:7766"
                );
                bool permitted = await permit.Check(
                    testUserId,
                    "{{diff_action}}",
                    "{{diff_resource}}"
                );
                if (permitted)
                {
                    await sendResponseAsync(
                        resp,
                        200,
                        String.Format(
                            pageData,
                            testUserId,
                            "Permitted",
                            "{{diff_action}}",
                            "{{diff_resource}}"
                        )
                    );
                }
                else
                {
                    await sendResponseAsync(
                        resp,
                        403,
                        String.Format(
                            pageData,
                            testUserId,
                            "NOT Permitted",
                            "{{diff_action}}",
                            "{{diff_resource}}"
                        )
                    );
                }
            }
        }

        public static async Task sendResponseAsync(
            HttpListenerResponse resp,
            int returnCode,
            string responseContent
        )
        {
            byte[] data = Encoding.UTF8.GetBytes(responseContent);
            resp.StatusCode = returnCode;
            await resp.OutputStream.WriteAsync(data, 0, data.Length);
            resp.Close();
        }

        public static void Main(string[] args)
        {
            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        }
    }
}

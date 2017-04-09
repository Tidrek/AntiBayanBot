using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NgrokHosting
{
    public class Program
    {
        private const string PathToNgrok = @"C:\Program Files\ngrok\ngrok.exe";

        public static void Main(string[] args)
        {
            try
            {
                Main().Wait();
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        public static async Task Main()
        {
            // Enable debug for ngrok?
            bool debug;
            while (true)
            {
                Console.Write("Inspect requests to ngrok (for debugging)? Type Y for inspect and N for no inspection. Default is N: ");
                var key = Console.ReadKey();

                Console.WriteLine();

                if(key.Key == ConsoleKey.N || key.Key == ConsoleKey.Enter)
                {
                    debug = false;
                    break;
                }
                if (key.Key == ConsoleKey.Y)
                {
                    debug = true;
                    break;
                }
            }

            // Ngrok process
            var process = new Process
            {
                StartInfo =
                {
                    FileName = PathToNgrok,
                    Arguments = debug ? "http 80" : "http -inspect=false 80",
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            try
            {
                Console.WriteLine("Starting ngrok " + (debug ? "in inspect mode." : "without inspect mode."));
                // Start Ngrok
                process.Start();
                Console.WriteLine("Ngrok started.");

                // Wait for ngrok to load
                Thread.Sleep(1000);

                var webClient = new WebClient();

                // Get page
                var pageContent = await webClient.GetNgrokPanelPage();

                // Parse URL from the page
                var ngrokUrl = NgrokPanelPageParser.GetNgrokUrl(pageContent);
                Console.WriteLine($"Ngrok URL: {ngrokUrl}");

                // Update web hook
                await webClient.UpdateWebHook(ngrokUrl);
                Console.WriteLine("Web hook URL was updated successfully.");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
                throw;
            }
        }
    }
}
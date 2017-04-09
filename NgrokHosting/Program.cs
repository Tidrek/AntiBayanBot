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
            // Ngrok process
            var process = new Process
            {
                StartInfo =
                {
                    FileName = PathToNgrok,
                    Arguments = "http 80",
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            try
            {
                Console.WriteLine("Starting ngrok.");
                // Start Ngrok
                process.Start();
                Console.WriteLine("Ngrok started.");

                // Wait for ngrok to load
                Thread.Sleep(1000);

                var webClient = new WebClient();

                // Get page
                var pageContent = await webClient.GetNgrokPanelPage();

                var ngrokUrl = NgrokPanelPageParser.GetNgrokUrl(pageContent);
                Console.WriteLine($"Ngrok URL: {ngrokUrl}");

                await webClient.UpdateWebHook(ngrokUrl);
                Console.WriteLine("Web hook URL was updated successfully.");

                while (true)
                {
                    Console.Write("Press any key to exit...");
                    Console.ReadKey();
                    process.Kill();
                    process.WaitForExit();
                    Environment.Exit(0);
                }
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
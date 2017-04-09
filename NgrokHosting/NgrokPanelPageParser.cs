using System;
using System.Text.RegularExpressions;

namespace NgrokHosting
{
    public static class NgrokPanelPageParser
    {
        public static string GetNgrokUrl(string pageContent)
        {
            var match = Regex.Match(pageContent, "\\\\\"URL\\\\\":\\\\\"https://(.*?)\\\\\"", RegexOptions.IgnoreCase);
            if (match.Success)
                return "https://" + match.Groups[1].Value;
            
            throw new Exception("Ngrok HTTPS url was not found in the page content.");
        }
    }
}
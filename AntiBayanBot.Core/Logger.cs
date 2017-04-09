using NLog;

namespace AntiBayanBot.Core
{
    public static class Logger
    {
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Info(string message)
        {
            _logger.Info(message);
        }
    }
}
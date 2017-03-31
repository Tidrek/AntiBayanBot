using System;
using System.Configuration;

namespace AntiBayanBot.Core
{
    public static class Settings
    {
        public static T Get<T>(string key)
        {
            var result = ConfigurationManager.AppSettings[key];

            if (result == null)
                throw new ConfigurationErrorsException($"The parameter \"{key}\" is not specified in the configuration.");
            
            // Convert and return
            return (T)Convert.ChangeType(result, typeof(T));
        }
    }
}
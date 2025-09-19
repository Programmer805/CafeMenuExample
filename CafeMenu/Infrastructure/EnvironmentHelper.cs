using System;
using System.Configuration;
using System.Web;

namespace CafeMenu.Infrastructure
{
    /// <summary>
    /// Environment helper class for managing different deployment environments
    /// </summary>
    public static class EnvironmentHelper
    {
        private static readonly string _environment;
        private static readonly bool _isDevelopment;
        private static readonly bool _isTest;
        private static readonly bool _isProduction;

        static EnvironmentHelper()
        {
            // Try to get environment from app settings first
            _environment = ConfigurationManager.AppSettings["Environment"] ?? "Development";
            
            // Fallback to machine name detection
            if (string.IsNullOrEmpty(_environment) || _environment == "Development")
            {
                var machineName = Environment.MachineName.ToLower();
                if (machineName.Contains("test") || machineName.Contains("staging"))
                {
                    _environment = "Test";
                }
                else if (machineName.Contains("prod") || machineName.Contains("live"))
                {
                    _environment = "Production";
                }
                else
                {
                    _environment = "Development";
                }
            }

            _isDevelopment = _environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
            _isTest = _environment.Equals("Test", StringComparison.OrdinalIgnoreCase);
            _isProduction = _environment.Equals("Production", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the current environment name
        /// </summary>
        public static string EnvironmentName => _environment;

        /// <summary>
        /// Returns true if running in Development environment
        /// </summary>
        public static bool IsDevelopment => _isDevelopment;

        /// <summary>
        /// Returns true if running in Test environment
        /// </summary>
        public static bool IsTest => _isTest;

        /// <summary>
        /// Returns true if running in Production environment
        /// </summary>
        public static bool IsProduction => _isProduction;

        /// <summary>
        /// Gets a configuration value with environment-specific fallback
        /// </summary>
        /// <param name="key">Configuration key</param>
        /// <param name="defaultValue">Default value if not found</param>
        /// <returns>Configuration value</returns>
        public static string GetConfigValue(string key, string defaultValue = null)
        {
            // Try environment-specific key first
            var envKey = $"{_environment}_{key}";
            var value = ConfigurationManager.AppSettings[envKey];
            
            // Fallback to generic key
            if (string.IsNullOrEmpty(value))
            {
                value = ConfigurationManager.AppSettings[key];
            }
            
            return value ?? defaultValue;
        }

        /// <summary>
        /// Gets a boolean configuration value
        /// </summary>
        /// <param name="key">Configuration key</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Boolean configuration value</returns>
        public static bool GetConfigBool(string key, bool defaultValue = false)
        {
            var value = GetConfigValue(key);
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets an integer configuration value
        /// </summary>
        /// <param name="key">Configuration key</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Integer configuration value</returns>
        public static int GetConfigInt(string key, int defaultValue = 0)
        {
            var value = GetConfigValue(key);
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets the connection string for the current environment
        /// </summary>
        /// <param name="name">Connection string name</param>
        /// <returns>Connection string</returns>
        public static string GetConnectionString(string name)
        {
            // Try environment-specific connection string first
            var envName = $"{_environment}_{name}";
            var connectionString = ConfigurationManager.ConnectionStrings[envName];
            
            // Fallback to generic connection string
            if (connectionString == null)
            {
                connectionString = ConfigurationManager.ConnectionStrings[name];
            }
            
            return connectionString?.ConnectionString;
        }

        /// <summary>
        /// Gets the log level for the current environment
        /// </summary>
        public static string LogLevel => GetConfigValue("LogLevel", IsDevelopment ? "Debug" : "Warning");

        /// <summary>
        /// Gets whether caching is enabled for the current environment
        /// </summary>
        public static bool IsCacheEnabled => GetConfigBool("CacheEnabled", IsProduction);

        /// <summary>
        /// Gets whether detailed errors should be shown
        /// </summary>
        public static bool ShowDetailedErrors => GetConfigBool("DetailedErrors", IsDevelopment || IsTest);
    }
}
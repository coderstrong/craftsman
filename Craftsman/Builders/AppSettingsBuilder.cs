﻿namespace Craftsman.Builders
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class AppSettingsBuilder
    {
        /// <summary>
        /// this build will create environment based app settings files.
        /// </summary>
        public static void CreateWebApiAppSettings(string solutionDirectory, ApiEnvironment env, string dbName, string projectBaseName)
        {
            var appSettingFilename = Utilities.GetAppSettingsName(env.EnvironmentName);
            var classPath = ClassPathHelper.WebApiAppSettingsClassPath(solutionDirectory, $"{appSettingFilename}", projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                File.Delete(classPath.FullClassPath);

            using FileStream fs = File.Create(classPath.FullClassPath);
            var data = GetAppSettingsText(env, dbName);
            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        public static void CreateAuthServerAppSettings(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerAppSettingsClassPath(projectDirectory, $"appsettings.json", authServerProjectName);
            var fileText = @$"{{
  ""AllowedHosts"": ""*""
}}
";
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string GetAppSettingsText(ApiEnvironment env, string dbName)
        {
            var jwtSettings = GetJwtAuthSettings(env);
            var serilogSettings = GetSerilogSettings(env.EnvironmentName);
            var bus = env.EnvironmentName == "FunctionalTesting" ? "true" : "false";

            if (env.EnvironmentName == "Development" || env.EnvironmentName == "FunctionalTesting")

                return @$"{{
  ""AllowedHosts"": ""*"",
  ""UseInMemoryBus"": {bus},
  ""UseInMemoryDatabase"": true,
{serilogSettings}{jwtSettings}
}}
";
            else
            {
                // won't build properly if it has an empty string
                var connectionString = String.IsNullOrEmpty(env.ConnectionString) ? "local" : env.ConnectionString.Replace(@"\", @"\\");
                return @$"{{
  ""AllowedHosts"": ""*"",
  ""UseInMemoryBus"": false,
  ""UseInMemoryDatabase"": false,
  ""ConnectionStrings"": {{
    ""{dbName}"": ""{connectionString}""
  }},
{serilogSettings}{jwtSettings}
}}
";
            }
        }

        private static string GetJwtAuthSettings(ApiEnvironment env)
        {
            return $@",
  ""JwtSettings"": {{
    ""Audience"": ""{env.Audience}"",
    ""Authority"": ""{env.Authority}"",
    ""AuthorizationUrl"": ""{env.AuthorizationUrl}"",
    ""TokenUrl"": ""{env.TokenUrl}"",
    ""ClientId"": ""{env.ClientId}"",
    ""ClientSecret"": ""{env.ClientSecret}""
  }}";
        }

        private static string GetSerilogSettings(string envName)
        {
            var writeTo = envName == "Development" || envName == "FunctionalTesting" ? $@"
      {{ ""Name"": ""Console"" }},
      {{
        ""Name"": ""Seq"",
        ""Args"": {{
          ""serverUrl"": ""http://localhost:5341""
        }}
      }}
    " : "";

            return $@"  ""Serilog"": {{
    ""Using"": [],
    ""MinimumLevel"": {{
      ""Default"": ""Information"",
      ""Override"": {{
        ""Microsoft"": ""Warning"",
        ""System"": ""Warning""
      }}
    }},
    ""Enrich"": [ ""FromLogContext"", ""WithMachineName"", ""WithProcessId"", ""WithThreadId"" ],
    ""WriteTo"": [{writeTo}]
  }}";
        }
    }
}
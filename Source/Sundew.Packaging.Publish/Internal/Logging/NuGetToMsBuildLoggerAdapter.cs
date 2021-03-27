// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetToMsBuildLoggerAdapter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Logging
{
    using System;
    using global::NuGet.Common;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Sundew.Packaging.Publish.Internal.Commands;

    internal class NuGetToMsBuildLoggerAdapter : global::NuGet.Common.ILogger
    {
        private readonly ILogger log;

        public NuGetToMsBuildLoggerAdapter(ILogger log)
        {
            this.log = log;
        }

        public void LogDebug(string data)
        {
            this.log.LogMessage($"SPP:NuGet Debug: {data}");
        }

        public void LogVerbose(string data)
        {
            this.log.LogMessage($"SPP:NuGet Verbose: {data}");
        }

        public void LogInformation(string data)
        {
            this.log.LogInfo($"SPP:NuGet Information: {data}");
        }

        public void LogMinimal(string data)
        {
            this.log.LogImportant($"SPP:NuGet Minimal: {data}");
        }

        public void LogWarning(string data)
        {
            this.log.LogWarning(data);
        }

        public void LogError(string data)
        {
            this.log.LogError(data);
        }

        public void LogInformationSummary(string data)
        {
            this.log.LogInfo($"SPP:NuGet Summary: {data}");
        }

        public void Log(LogLevel level, string data)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    this.LogDebug(data);
                    break;
                case LogLevel.Verbose:
                    this.LogVerbose(data);
                    break;
                case LogLevel.Information:
                    this.LogInformation(data);
                    break;
                case LogLevel.Minimal:
                    this.LogMinimal(data);
                    break;
                case LogLevel.Warning:
                    this.LogWarning(data);
                    break;
                case LogLevel.Error:
                    this.LogError(data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        public System.Threading.Tasks.Task LogAsync(LogLevel level, string data)
        {
            this.Log(level, data);
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public void Log(ILogMessage message)
        {
            this.Log(message.Level, message.Message);
        }

        public System.Threading.Tasks.Task LogAsync(ILogMessage message)
        {
            this.Log(message.Level, message.Message);
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
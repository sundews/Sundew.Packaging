// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetToLoggerAdapter.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.Logging;

using System;
using global::NuGet.Common;

/// <summary>
/// Adapts NuGet log messages to MSBuild.
/// </summary>
public class NuGetToLoggerAdapter : global::NuGet.Common.ILogger
{
    private readonly Versioning.Logging.ILogger log;

    /// <summary>
    /// Initializes a new instance of the <see cref="NuGetToLoggerAdapter"/> class.
    /// </summary>
    /// <param name="log">The log.</param>
    public NuGetToLoggerAdapter(Versioning.Logging.ILogger log)
    {
        this.log = log;
    }

    /// <summary>
    /// Logs the debug.
    /// </summary>
    /// <param name="data">The data.</param>
    public void LogDebug(string data)
    {
        this.log.LogMessage($"SPP:NuGet Debug: {data}");
    }

    /// <summary>
    /// Logs the verbose.
    /// </summary>
    /// <param name="data">The data.</param>
    public void LogVerbose(string data)
    {
        this.log.LogMessage($"SPP:NuGet Verbose: {data}");
    }

    /// <summary>
    /// Logs the information.
    /// </summary>
    /// <param name="data">The data.</param>
    public void LogInformation(string data)
    {
        this.log.LogInfo($"SPP:NuGet Information: {data}");
    }

    /// <summary>
    /// Logs the minimal.
    /// </summary>
    /// <param name="data">The data.</param>
    public void LogMinimal(string data)
    {
        this.log.LogImportant($"SPP:NuGet Minimal: {data}");
    }

    /// <summary>
    /// Logs the warning.
    /// </summary>
    /// <param name="data">The data.</param>
    public void LogWarning(string data)
    {
        this.log.LogWarning(data);
    }

    /// <summary>
    /// Logs the error.
    /// </summary>
    /// <param name="data">The data.</param>
    public void LogError(string data)
    {
        this.log.LogError(data);
    }

    /// <summary>
    /// Logs the information summary.
    /// </summary>
    /// <param name="data">The data.</param>
    public void LogInformationSummary(string data)
    {
        this.log.LogInfo($"SPP:NuGet Summary: {data}");
    }

    /// <summary>
    /// Logs the specified level.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="data">The data.</param>
    /// <exception cref="ArgumentOutOfRangeException">level - null.</exception>
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

    /// <summary>
    /// Logs the asynchronous.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="data">The data.</param>
    /// <returns>An async task.</returns>
    public System.Threading.Tasks.Task LogAsync(LogLevel level, string data)
    {
        this.Log(level, data);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    /// <summary>
    /// Logs the specified message.
    /// </summary>
    /// <param name="message">The message.</param>
    public void Log(ILogMessage message)
    {
        this.Log(message.Level, message.Message);
    }

    /// <summary>
    /// Logs the asynchronous.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>An async task.</returns>
    public System.Threading.Tasks.Task LogAsync(ILogMessage message)
    {
        this.Log(message.Level, message.Message);
        return System.Threading.Tasks.Task.CompletedTask;
    }
}
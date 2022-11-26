// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageVersionLogger.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Versioning;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using global::NuGet.Versioning;
using Sundew.Base.Memory;
using Sundew.Base.Text;
using Sundew.Packaging.Versioning.IO;

/// <summary>
/// Logs information about a package version.
/// </summary>
public sealed class PackageVersionLogger
{
    private const string DoubleQuotes = @"""";
    private const string IndicesContainedNullValues = "The following indices contained null values: ";
    private const string UnknownNames = "The following name(s) where not found: ";
    private const string LogFormat = "LogFormat";
    private const string FilePath = "FilePath";
    private static readonly string[] LogNames = new[] { "PackageId", "Version", "FullVersion", "Stage", "VersionStage", "PushSource", "ApiKey", "FeedSource", "SymbolsPushSource", "SymbolsApiKey", "Metadata", "WorkingDirectory", "Parameter", "VersionMajor", "VersionMinor", "VersionPatch", "VersionRevision", "VersionRelease", "DQ", "NL" };
    private static readonly Regex RedirectFormat = new Regex(@"^(?:>(?<FilePath>[^\|]+)?\|)(?<LogFormat>.*)");
    private readonly IStageBuildLogger logger;
    private readonly IFileSystem fileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackageVersionLogger" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="fileSystem">The file system.</param>
    public PackageVersionLogger(IStageBuildLogger logger, IFileSystem fileSystem)
    {
        this.logger = logger;
        this.fileSystem = fileSystem;
    }

    /// <summary>
    /// Logs the specified log formats.
    /// </summary>
    /// <param name="logFormats">The log formats.</param>
    /// <param name="packageId">The package identifier.</param>
    /// <param name="publishInfo">The publish information.</param>
    /// <param name="workingDirectory">The working directory.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="nuGetVersion">The nuget version.</param>
    /// <param name="properties">The properties.</param>
    /// <param name="outputFilePath">The output file path.</param>
    public void Log(
        IReadOnlyList<string>? logFormats,
        string packageId,
        PublishInfo publishInfo,
        string workingDirectory,
        string parameter,
        NuGetVersion? nuGetVersion,
        IReadOnlyDictionary<string, string>? properties,
        string? outputFilePath)
    {
        if (logFormats == null)
        {
            return;
        }

        var valueBuffer = new Buffer<object?>(LogNames.Length + properties?.Count ?? 0);
        valueBuffer.Write(packageId);
        valueBuffer.Write(publishInfo.Version);
        valueBuffer.Write(publishInfo.FullVersion);
        valueBuffer.Write(publishInfo.Stage);
        valueBuffer.Write(publishInfo.VersionStage);
        valueBuffer.Write(publishInfo.PushSource);
        valueBuffer.Write(publishInfo.ApiKey);
        valueBuffer.Write(publishInfo.FeedSource);
        valueBuffer.Write(publishInfo.SymbolsPushSource);
        valueBuffer.Write(publishInfo.SymbolsApiKey);
        valueBuffer.Write(publishInfo.Metadata);
        valueBuffer.Write(workingDirectory);
        valueBuffer.Write(parameter);
        valueBuffer.Write(nuGetVersion != null ? nuGetVersion.Major : null);
        valueBuffer.Write(nuGetVersion != null ? nuGetVersion.Minor : null);
        valueBuffer.Write(nuGetVersion != null ? nuGetVersion.Patch : null);
        valueBuffer.Write(nuGetVersion != null ? nuGetVersion.Revision : null);
        valueBuffer.Write(nuGetVersion != null ? nuGetVersion.Release : null);
        valueBuffer.Write(DoubleQuotes);
        valueBuffer.Write(Environment.NewLine);
        var logNames = LogNames.ToList();
        if (properties != null)
        {
            foreach (var property in properties)
            {
                logNames.Add(property.Key);
                valueBuffer.Write(property.Value);
            }
        }

        foreach (var logFormat in logFormats)
        {
            var match = RedirectFormat.Match(logFormat);
            if (match.Success)
            {
                var filePath = match.Groups[FilePath].Value;
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = outputFilePath;
                }

                if (string.IsNullOrEmpty(filePath))
                {
                    this.logger.ReportMessage($"The log format {logFormat} did not specify a file path and no fallback path was specified.");
                    return;
                }

                var (log, isValid) = Format(match.Groups[LogFormat].Value, logNames, valueBuffer.ToArray());
                if (isValid)
                {
                    this.fileSystem.AppendAllText(filePath, log);
                }
                else
                {
                    this.logger.ReportMessage(log);
                }
            }
            else
            {
                var (log, isValid) = Format(logFormat, logNames, valueBuffer.ToArray());
                if (isValid)
                {
                    this.logger.ReportMessage(log);
                }
                else
                {
                    this.logger.ReportMessage(log);
                }
            }
        }
    }

    internal static (string Log, bool IsValid) Format(
        string logFormat,
        IReadOnlyList<string> logNames,
        object?[] arguments)
    {
        const string separator = ", ";
        if (NamedFormatString.TryCreate(logFormat, logNames, out var namedFormatString, out var unknownNames))
        {
            var nullArguments = namedFormatString.GetNullArguments(arguments);
            if (nullArguments.Count > 0)
            {
                return (nullArguments.JoinToStringBuilder(new StringBuilder(IndicesContainedNullValues), (builder, namedIndex) => builder.Append($"{namedIndex.Name}({namedIndex.Index})"), separator).ToString(), false);
            }

            return (string.Format(CultureInfo.CurrentCulture, namedFormatString, arguments), true);
        }

        return (unknownNames.JoinToStringBuilder(new StringBuilder(UnknownNames), (builder, name) => builder.Append(name), separator).ToString(), false);
    }
}
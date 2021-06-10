// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageVersionLogger.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Versioning
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Sundew.Base.Memory;
    using Sundew.Base.Text;
    using Sundew.Packaging.Versioning;
    using Sundew.Packaging.Versioning.IO;

    /// <summary>
    /// Logs information about a package version.
    /// </summary>
    public sealed class PackageVersionLogger
    {
        private const string DoubleQuotes = @"""";
        private const string IndicesContainedNullValues = "The following indices contained null values: ";
        private const string UnknownNames = "The following name(s) where not found: ";
        private static readonly string[] LogNames = new[] { "PackageId", "Version", "Stage", "PackageStage", "PushSource", "ApiKey", "FeedSource", "SymbolsPushSource", "SymbolsApiKey", "Metadata", "CurrentDirectory", "Parameter", "DQ", "NL" };
        private readonly IFileSystem fileSystem;
        private readonly IStageBuildLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageVersionLogger" /> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="logger">The logger.</param>
        public PackageVersionLogger(IFileSystem fileSystem, IStageBuildLogger logger)
        {
            this.fileSystem = fileSystem;
            this.logger = logger;
        }

        /// <summary>
        /// Logs the specified log formats.
        /// </summary>
        /// <param name="logFormats">The log formats.</param>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="publishInfo">The publish information.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="properties">The properties.</param>
        public void Log(
            IReadOnlyList<string>? logFormats,
            string packageId,
            PublishInfo publishInfo,
            string parameter,
            IReadOnlyDictionary<string, string>? properties)
        {
            if (logFormats == null)
            {
                return;
            }

            var valueBuffer = new Buffer<object?>(LogNames.Length + properties?.Count ?? 0);
            valueBuffer.Write(packageId);
            valueBuffer.Write(publishInfo.Version);
            valueBuffer.Write(publishInfo.Stage);
            valueBuffer.Write(publishInfo.VersionStage);
            valueBuffer.Write(publishInfo.PushSource);
            valueBuffer.Write(publishInfo.ApiKey);
            valueBuffer.Write(publishInfo.FeedSource);
            valueBuffer.Write(publishInfo.SymbolsPushSource);
            valueBuffer.Write(publishInfo.SymbolsApiKey);
            valueBuffer.Write(publishInfo.Metadata);
            valueBuffer.Write(this.fileSystem.GetCurrentDirectory());
            valueBuffer.Write(parameter);
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
                var (log, isValid) = Format(logFormat.ToString(), logNames, valueBuffer.ToArray());
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
}
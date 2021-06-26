// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackagePublicationLogger.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    using System;
    using System.Globalization;
    using System.Text;
    using Sundew.Base.Memory;
    using Sundew.Base.Text;
    using Sundew.Packaging.Versioning;
    using Sundew.Packaging.Versioning.Logging;

    internal sealed class PackagePublicationLogger
    {
        private const string DoubleQuotes = @"""";
        private const string IndicesContainedNullValues = "The following indices contained null values: ";
        private static readonly string[] LogNames = new[] { "PackageId", "Version", "FullVersion", "PackagePath", "Stage", "VersionStage", "PushSource", "ApiKey", "FeedSource", "SymbolsPath", "SymbolsPushSource", "SymbolsApiKey", "Metadata", "WorkingDirectory", "Parameter", "DQ", "NL" };
        private readonly ILogger logger;

        public PackagePublicationLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public void Log(
            string packagePushLogFormats,
            string packageId,
            string packagePath,
            string? symbolPackagePath,
            PublishInfo publishInfo,
            string workingDirectory,
            string parameter)
        {
            const char pipe = '|';
            var lastWasPipe = false;
            var logFormats = packagePushLogFormats.AsMemory().Split(
                (character, _, _) =>
                {
                    var wasPipe = lastWasPipe;
                    if (wasPipe)
                    {
                        lastWasPipe = false;
                    }

                    switch (character)
                    {
                        case pipe:
                            if (wasPipe)
                            {
                                return SplitAction.Include;
                            }

                            lastWasPipe = true;
                            return SplitAction.Ignore;
                        default:
                            if (wasPipe)
                            {
                                return SplitAction.SplitAndInclude;
                            }

                            return SplitAction.Include;
                    }
                },
                SplitOptions.RemoveEmptyEntries);
            foreach (var logFormat in logFormats)
            {
                var (log, isValid) = Format(logFormat.ToString(), packageId, packagePath, symbolPackagePath, publishInfo, workingDirectory, parameter);
                if (isValid)
                {
                    this.logger.LogImportant(log);
                }
                else
                {
                    this.logger.LogWarning(log);
                }
            }
        }

        internal static (string Log, bool IsValid) Format(
            string logFormat,
            string packageId,
            string packagePath,
            string? symbolPackagePath,
            PublishInfo publishInfo,
            string workingDirectory,
            string parameter)
        {
            var arguments = new object?[]
            {
                packageId, publishInfo.Version, publishInfo.FullVersion, packagePath, publishInfo.Stage, publishInfo.VersionStage, publishInfo.PushSource, publishInfo.ApiKey, publishInfo.FeedSource, symbolPackagePath, publishInfo.SymbolsPushSource, publishInfo.SymbolsApiKey, publishInfo.Metadata, workingDirectory, parameter, DoubleQuotes, Environment.NewLine,
            };

            var namedFormatString = new NamedFormatString(logFormat, LogNames);
            var nullArguments = namedFormatString.GetNullArguments(arguments);
            if (nullArguments.Count > 0)
            {
                const string separator = ", ";
                return (nullArguments.JoinToStringBuilder(new StringBuilder(IndicesContainedNullValues), (builder, namedIndex) => builder.Append($"{namedIndex.Name}({namedIndex.Index})"), separator).ToString(), false);
            }

            return (string.Format(CultureInfo.CurrentCulture, namedFormatString, arguments), true);
        }
    }
}
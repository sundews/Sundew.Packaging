// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublishLogger.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Sundew.Base.Collections;
    using Sundew.Base.Text;
    using Sundew.Packaging.Publish.Internal.Logging;

    internal static class PublishLogger
    {
        private const string DoubleQuotes = @"""";
        private const string IndexGroupName = "Index";
        private const string IndicesContainedNullValues = "The following indices contained null values: ";
        private static readonly Regex FormatRegex = new(@"[^\{\}]*(?:(?>(?<CurlyOpen>\{\{)|\{)(?<Index>\d+)(?:[\+\-\.\,\:\w\d]*)(?>(?<CurlyClosed-CurlyOpen>\}\})|\})(?(CurlyOpen)(?!))(?(CurlyClosed)(?<-Index>)(?<-CurlyClosed>))[^\{\}]*)+");

        public static void Log(
            ILogger logger,
            string packagePushLogFormats,
            string packageId,
            string packagePath,
            string? symbolPackagePath,
            PublishInfo publishInfo,
            string parameter)
        {
            const char pipe = '|';
            var lastWasPipe = false;
            var logFormats = packagePushLogFormats.Split(
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
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var logFormat in logFormats)
            {
                var (log, isValid) = Format(logFormat, packageId, packagePath, symbolPackagePath, publishInfo, parameter);
                if (isValid)
                {
                    logger.LogImportant(log);
                }
                else
                {
                    logger.LogWarning(log);
                }
            }
        }

        internal static (string Log, bool IsValid) Format(
            string logFormat,
            string packageId,
            string packagePath,
            string? symbolPackagePath,
            PublishInfo publishInfo,
            string parameter)
        {
            var arguments = new object?[]
            {
                packageId, publishInfo.Version, packagePath, publishInfo.Stage, publishInfo.PushSource, publishInfo.ApiKey, publishInfo.FeedSource, symbolPackagePath, publishInfo.SymbolsPushSource, publishInfo.SymbolsApiKey, parameter, DoubleQuotes, Environment.NewLine,
            };
            var stopWatch = Stopwatch.StartNew();
            var match = FormatRegex.Match(logFormat);
            var e = stopWatch.Elapsed;
            if (match.Success)
            {
                var indicesWithNullValues = match.Groups[IndexGroupName].Captures.Cast<Capture>().Select(x => int.Parse(x.Value)).Where(x => arguments[x] == null).ToReadOnly();
                if (indicesWithNullValues.Count > 0)
                {
                    const string separator = ", ";
                    return (indicesWithNullValues.JoinToStringBuilder(new StringBuilder(IndicesContainedNullValues), separator, CultureInfo.InvariantCulture).ToString(), false);
                }
            }

            return (string.Format(CultureInfo.CurrentCulture, logFormat, arguments), true);
        }
    }
}
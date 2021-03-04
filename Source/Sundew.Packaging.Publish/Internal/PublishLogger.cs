// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublishLogger.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    using System;
    using System.Globalization;
    using Sundew.Base.Text;
    using Sundew.Packaging.Publish.Internal.Commands;

    internal static class PublishLogger
    {
        private const string DoubleQuotes = @"""";

        public static void Log(
            ICommandLogger commandLogger,
            string packagePushLogFormats,
            string packageId,
            string version,
            string packagePath,
            string stage,
            string source,
            string? apiKey,
            string feedSource,
            string? symbolPackagePath,
            string? symbolsSource,
            string? symbolApiKey,
            string parameter)
        {
            const char pipe = '|';
            var lastWasPipe = false;
            var logFormats = packagePushLogFormats.Split(
                (char character, int index) =>
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
                commandLogger.LogImportant(Format(logFormat, packageId, version, packagePath, stage, source, apiKey, feedSource, symbolPackagePath, symbolsSource, symbolApiKey, parameter));
            }
        }

        internal static string Format(
            string logFormat,
            string packageId,
            string version,
            string packagePath,
            string stage,
            string source,
            string? apiKey,
            string feedSource,
            string? symbolPackagePath,
            string? symbolsSource,
            string? symbolApiKey,
            string parameter)
        {
            return string.Format(CultureInfo.CurrentCulture, logFormat, packageId, version, packagePath, stage, source, apiKey, feedSource, symbolPackagePath, symbolsSource, symbolApiKey, parameter, DoubleQuotes);
        }
    }
}
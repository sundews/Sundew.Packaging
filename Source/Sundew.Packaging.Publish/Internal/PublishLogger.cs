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
        public static void Log(ICommandLogger commandLogger, string packagePushLogFormats, string commandPrefix, string packageId, string version, string packagePath, string source, string? apiKey, string? symbolPackagePath, string? symbolsSource, string? symbolApiKey)
        {
            const char pipe = '|';
            var lastWasPipe = false;
            var logFormats = packagePushLogFormats.Split(
                (char character, int index) =>
                {
                    var wasSemiColon = lastWasPipe;
                    if (wasSemiColon)
                    {
                        lastWasPipe = false;
                    }

                    switch (character)
                    {
                        case pipe:
                            if (wasSemiColon)
                            {
                                return SplitAction.Include;
                            }

                            lastWasPipe = true;
                            return SplitAction.Ignore;
                        default:
                            if (wasSemiColon)
                            {
                                return SplitAction.SplitAndInclude;
                            }

                            return SplitAction.Include;
                    }
                },
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var logFormat in logFormats)
            {
                commandLogger.LogImportant(string.Format(CultureInfo.CurrentCulture, logFormat, commandPrefix, packageId, version, packagePath, source, apiKey, symbolPackagePath, symbolsSource, symbolApiKey));
            }
        }
    }
}
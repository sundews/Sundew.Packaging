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
        public static void Log(ICommandLogger commandLogger, string packagePushLogFormats, string packageId, string version, string source, string packagePath)
        {
            const char semiColon = '|';
            var lastWasSemiColon = false;
            var logFormats = packagePushLogFormats.Split(
                (char character, int index) =>
                {
                    var wasSemiColon = lastWasSemiColon;
                    if (wasSemiColon)
                    {
                        lastWasSemiColon = false;
                    }

                    switch (character)
                    {
                        case semiColon:
                            if (wasSemiColon)
                            {
                                return SplitAction.Include;
                            }

                            lastWasSemiColon = true;
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
                commandLogger.LogImportant(string.Format(CultureInfo.CurrentCulture, logFormat, packageId, version, source, packagePath));
            }
        }
    }
}
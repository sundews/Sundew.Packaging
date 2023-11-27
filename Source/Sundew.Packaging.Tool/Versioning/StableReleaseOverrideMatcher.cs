// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StableReleaseOverrideMatcher.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Versioning
{
    using System.Text.RegularExpressions;
    using Sundew.Base.Text;
    using Sundew.Packaging.Versioning.IO;

    public static class StableReleaseOverrideMatcher
    {
        public static bool IsStableRelease(
            string? productionInput,
            string? productionMatcherRegex,
            IFileSystem fileSystem,
            IStageBuildLogger stageBuildLogger)
        {
            if (productionInput.IsNullOrEmpty() || productionMatcherRegex.IsNullOrEmpty())
            {
                return false;
            }

            if (productionInput[0] == '<')
            {
                productionInput = fileSystem.ReadAllText(productionInput.Substring(1).Trim());
            }

            stageBuildLogger.ReportMessage(@$"Matching ""{productionInput}"" to ""{productionMatcherRegex}""");
            var isStableRelease = Regex.IsMatch(productionInput, productionMatcherRegex);
            if (isStableRelease)
            {
                stageBuildLogger.ReportMessage("Setting production stage");
            }

            return isStableRelease;
        }
    }
}
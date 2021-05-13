// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageVersioner.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    using System;
    using System.Collections.Generic;
    using global::NuGet.Common;
    using global::NuGet.Versioning;

    internal interface IPackageVersioner
    {
        SemanticVersion GetVersion(
            string packageId,
            NuGetVersion nuGetVersion,
            string? combinedVersion,
            string? forceVersion,
            VersioningMode versioningMode,
            SelectedSource selectedSource,
            IReadOnlyList<string> latestVersionSources,
            DateTime buildDateTime,
            string? metadata,
            string? metadataFormat,
            string parameter,
            ILogger nuGetLogger,
            Logging.ILogger logger);
    }
}
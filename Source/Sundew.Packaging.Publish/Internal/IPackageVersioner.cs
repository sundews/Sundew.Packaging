// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageVersioner.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    using global::NuGet.Common;
    using global::NuGet.Versioning;

    internal interface IPackageVersioner
    {
        SemanticVersion GetVersion(string packageId, NuGetVersion nuGetVersion, VersioningMode versioningMode, bool isStableRelease, Source source, ILogger logger);
    }
}
﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPrereleaseVersioner.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Internal
{
    using global::NuGet.Common;
    using global::NuGet.Versioning;

    internal interface IPrereleaseVersioner
    {
        SemanticVersion GetPrereleaseVersion(string packageId, SemanticVersion semanticVersion, PrereleaseVersioningMode prereleaseVersioningMode, Source source, ILogger logger);
    }
}
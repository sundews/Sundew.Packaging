// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILatestVersionSourcesCommand.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.Commands;

using System.Collections.Generic;
using global::NuGet.Configuration;
using Sundew.Packaging.Staging;

internal interface ILatestVersionSourcesCommand
{
    IReadOnlyList<PackageSource> GetLatestVersionSources(
        string? latestVersionSourcesText,
        SelectedStage selectedSource,
        NuGetSettings nuGetSettings,
        bool addNuGetOrgSource,
        bool addAllSources);
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILatestVersionSourcesCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.Commands
{
    using System.Collections.Generic;
    using Sundew.Packaging.Staging;

    internal interface ILatestVersionSourcesCommand
    {
        IReadOnlyList<string> GetLatestVersionSources(
            string? latestVersionSourcesText,
            SelectedStage selectedSource,
            NuGetSettings nuGetSettings,
            bool addNuGetOrgSource,
            bool addAllSources);
    }
}
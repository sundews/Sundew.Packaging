// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILatestVersionSourcesCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands
{
    using System.Collections.Generic;

    internal interface ILatestVersionSourcesCommand
    {
        IReadOnlyList<string> GetLatestVersionSources(
            string? latestVersionSourcesText,
            SelectedSource selectedSource,
            NuGetSettings nuGetSettings,
            bool addNuGetOrgSource,
            bool addAllSources);
    }
}
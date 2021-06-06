// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MsBuildProject.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Update.MsBuild
{
    using System.Collections.Generic;
    using Sundew.Packaging.Tool.Update.MsBuild.NuGet;

    public record MsBuildProject(string Path, string ProjectContent, IReadOnlyList<PackageUpdateSuggestion> PossiblePackageUpdates);
}
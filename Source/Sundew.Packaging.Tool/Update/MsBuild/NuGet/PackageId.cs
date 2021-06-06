// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageId.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Update.MsBuild.NuGet
{
    using System.Text.RegularExpressions;
    using global::NuGet.Versioning;
    using Sundew.Packaging.Tool.RegularExpression;

    public record PackageId(string Id, string? VersionPattern = null);

    public record PackageIdAndVersion(string Id, NuGetVersion NuGetVersion);

    public record PackageUpdateSuggestion(
        string Id,
        NuGetVersion NuGetVersion,
        GlobRegex? GlobRegex) : PackageIdAndVersion(Id, NuGetVersion);

    public record PackageUpdate(string Id, NuGetVersion NuGetVersion, NuGetVersion UpdatedNuGetVersion) : PackageIdAndVersion(Id, NuGetVersion);

    public record VersionMatcher(Regex Regex, string Pattern);
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageVersionSelectorReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Update.MsBuild.NuGet
{
    using global::NuGet.Versioning;

    public interface IPackageVersionSelectorReporter
    {
        void PackageUpdateSelected(string packageId, NuGetVersion? oldNuGetVersion, NuGetVersion newNuGetVersion);
    }
}
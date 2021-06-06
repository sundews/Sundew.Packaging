// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INuGetPackageVersionFetcher.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Update.MsBuild.NuGet
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::NuGet.Versioning;

    public interface INuGetPackageVersionFetcher
    {
        Task<IEnumerable<NuGetVersion>> GetAllVersionsAsync(string rootDirectory, string? source, string packageId);
    }
}
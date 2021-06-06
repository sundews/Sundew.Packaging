// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILatestPackageVersionCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.Commands
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::NuGet.Versioning;

    public interface ILatestPackageVersionCommand
    {
        Task<NuGetVersion?> GetLatestMajorMinorVersion(
            string packageId,
            IReadOnlyList<string> sources,
            NuGetVersion nuGetVersion,
            bool includePatchInMatch,
            bool allowPrerelease);
    }
}
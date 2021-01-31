// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILatestPackageVersionCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands
{
    using System.Threading.Tasks;
    using global::NuGet.Common;
    using global::NuGet.Versioning;

    internal interface ILatestPackageVersionCommand
    {
        Task<NuGetVersion?> GetLatestVersion(string packageId, string sourceUri, NuGetVersion nuGetVersion, bool allowPrerelease, ILogger logger);
    }
}
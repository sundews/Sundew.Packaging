// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LatestPackageVersionCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::NuGet.Common;
    using global::NuGet.Configuration;
    using global::NuGet.Protocol;
    using global::NuGet.Protocol.Core.Types;
    using global::NuGet.Versioning;

    internal class LatestPackageVersionCommand : ILatestPackageVersionCommand
    {
        public async Task<NuGetVersion?> GetLatestVersion(string packageId, string sourceUri, NuGetVersion nuGetVersion, bool allowPrerelease, ILogger logger)
        {
            PackageSource packageSource = new(sourceUri);
            var resourceAsync = await Repository.Factory.GetCoreV3(packageSource.Source).GetResourceAsync<FindPackageByIdResource>(CancellationToken.None).ConfigureAwait(false);
            if (resourceAsync != null)
            {
                return (await resourceAsync.GetAllVersionsAsync(
                        packageId,
                        new SourceCacheContext { NoCache = true, RefreshMemoryCache = true },
                        logger,
                        CancellationToken.None).ConfigureAwait(false))
                    .OrderByDescending(x => x)
                    .FirstOrDefault(x => x.Major == nuGetVersion.Major && x.Minor == nuGetVersion.Minor && (!x.IsPrerelease || (allowPrerelease && x.IsPrerelease)));
            }

            throw new NotSupportedException($"{nameof(RemoteV3FindPackageByIdResource)} not supported.");
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemotePackageExistsCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Internal.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::NuGet.Common;
    using global::NuGet.Configuration;
    using global::NuGet.Protocol;
    using global::NuGet.Protocol.Core.Types;
    using global::NuGet.Versioning;

    internal class RemotePackageExistsCommand : IPackageExistsCommand
    {
        public async Task<bool> ExistsAsync(string packageId, SemanticVersion semanticVersion, string sourceUri, ILogger logger)
        {
            PackageSource packageSource = new PackageSource(sourceUri);
            var resourceAsync = await Repository.Factory.GetCoreV3(packageSource.Source).GetResourceAsync<FindPackageByIdResource>(CancellationToken.None).ConfigureAwait(false);
            if (resourceAsync != null)
            {
                return await resourceAsync.DoesPackageExistAsync(
                    packageId,
                    new NuGetVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch, semanticVersion.Release),
                    new NullSourceCacheContext(),
                    logger,
                    CancellationToken.None).ConfigureAwait(false);
            }

            throw new NotSupportedException($"{nameof(RemoteV3FindPackageByIdResource)} not supported.");
        }
    }
}
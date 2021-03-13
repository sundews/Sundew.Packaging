// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LatestPackageVersionCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using global::NuGet.Common;
    using global::NuGet.Configuration;
    using global::NuGet.Protocol;
    using global::NuGet.Protocol.Core.Types;
    using global::NuGet.Versioning;
    using Sundew.Base.Collections;
    using Sundew.Base.Text;

    internal class LatestPackageVersionCommand : ILatestPackageVersionCommand
    {
        private const string Separator = ", ";
        private const string DeterminingLatestVersionFromSources = "Determining latest version from sources: ";

        public async Task<NuGetVersion?> GetLatestMajorMinorVersion(string packageId, IReadOnlyList<string> sources, NuGetVersion nuGetVersion, bool includePatchInMatch, bool allowPrerelease, ILogger logger)
        {
            logger.LogInformation(sources.AggregateToStringBuilder(new StringBuilder(DeterminingLatestVersionFromSources), (builder, s) => builder.Append(s).Append(Separator), builder => builder.ToStringFromEnd(Separator.Length)));
            return (await sources.SelectAsync(async sourceUri =>
                {
                    PackageSource packageSource = new(sourceUri);
                    var resourceAsync = await Repository.Factory.GetCoreV3(packageSource.Source)
                        .GetResourceAsync<FindPackageByIdResource>(CancellationToken.None).ConfigureAwait(false);
                    return await resourceAsync.GetAllVersionsAsync(
                        packageId,
                        new SourceCacheContext { NoCache = true, RefreshMemoryCache = true },
                        logger,
                        CancellationToken.None).ConfigureAwait(false);
                }))
                .SelectMany(x => x)
                .OrderByDescending(x => x)
                .FirstOrDefault(x =>
                    x.Major == nuGetVersion.Major
                    && x.Minor == nuGetVersion.Minor
                    && (x.Patch == nuGetVersion.Patch || !includePatchInMatch)
                    && (!x.IsPrerelease || (allowPrerelease && x.IsPrerelease)));
        }
    }
}
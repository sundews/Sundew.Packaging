// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetPackageVersionFetcher.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Update.MsBuild.NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::NuGet.Common;
    using global::NuGet.Configuration;
    using global::NuGet.Protocol;
    using global::NuGet.Protocol.Core.Types;
    using global::NuGet.Versioning;
    using Sundew.Base.Collections;
    using Sundew.Packaging.Tool.NuGet;

    public class NuGetPackageVersionFetcher : INuGetPackageVersionFetcher
    {
        private const string All = "All";
        private readonly INuGetSourceProvider nuGetSourceProvider;

        public NuGetPackageVersionFetcher(INuGetSourceProvider nuGetSourceProvider)
        {
            this.nuGetSourceProvider = nuGetSourceProvider;
        }

        public async Task<IEnumerable<NuGetVersion>> GetAllVersionsAsync(string rootDirectory, string? source, string packageId)
        {
            var logger = NullLogger.Instance;
            var cancellationToken = CancellationToken.None;
            var sourceSettings = this.nuGetSourceProvider.GetSourceSettings(rootDirectory, source);
            var sources = source == All
                ? sourceSettings.PackageSourcesSection?.Items.OfType<AddItem>().Select(x => x.Value) ?? throw new InvalidOperationException($"No package sources were found for: {source}")
                : new[] { sourceSettings.Source ?? throw new InvalidOperationException($"A source for: {source} was not found.") };

            return (await sources.SelectAsync(async x =>
                {
                    var sourceRepository = Repository.Factory.GetCoreV3(x);
                    var resource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancellationToken).ConfigureAwait(false);

                    return await resource.GetAllVersionsAsync(
                        packageId,
                        new SourceCacheContext { NoCache = true, RefreshMemoryCache = true },
                        logger,
                        cancellationToken).ConfigureAwait(false);
                }))
                .SelectMany(x => x);
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewestPrereleasesPruneFacade.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.PruneLocalSource
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::NuGet.Common;
    using global::NuGet.Packaging.Core;
    using global::NuGet.Protocol;
    using global::NuGet.Protocol.Core.Types;
    using Sundew.Base.Collections;
    using Sundew.Packaging.Tool.NuGet;

    public class NewestPrereleasesPruneFacade
    {
        private readonly INuGetSourceProvider nuGetSourceProvider;

        public NewestPrereleasesPruneFacade(INuGetSourceProvider nuGetSourceProvider)
        {
            this.nuGetSourceProvider = nuGetSourceProvider;
        }

        public async Task PruneAsync(NewestPrereleasesPruneModeVerb newestPrereleasesPurgeModeVerb)
        {
            var source = this.nuGetSourceProvider.GetDefaultSource(newestPrereleasesPurgeModeVerb.Source);
            if (!UriUtility.TryCreateSourceUri(source, UriKind.Absolute).IsFile)
            {
                throw new InvalidOperationException("Purge only works with local sources");
            }

            var sourceRepository = Repository.Factory.GetCoreV3(source);
            var cancellationToken = CancellationToken.None;
            var packageSearchResourceV3 = await sourceRepository.GetResourceAsync<PackageSearchResource>(cancellationToken).ConfigureAwait(false);
            var packages = (await newestPrereleasesPurgeModeVerb.PackageIds
                        .Select(x =>
                            {
                                if (x.EndsWith("*"))
                                {
                                    return (id: x.Substring(0, x.Length - 1), count: int.MaxValue);
                                }

                                return (id: x, count: 1);
                            })
                    .SelectAsync(async idAndCount => (await
                        packageSearchResourceV3.SearchAsync(
                            idAndCount.id,
                            new SearchFilter(true),
                            0,
                            idAndCount.count,
                            NullLogger.Instance,
                            cancellationToken).ConfigureAwait(false))).ConfigureAwait(false))
                .SelectMany(x => x).ToList();
            var packageIdentities = (await packages.SelectAsync(async x => (package: x, versions: (await x.GetVersionsAsync().ConfigureAwait(false))
                    .OrderByDescending(x => x.Version)
                    .TakeWhile(x => x.Version.IsPrerelease))).ConfigureAwait(false))
                    .Select(x => x.versions.Select(y => new PackageIdentity(x.package.Identity.Id, y.Version)))
                    .SelectMany(x => x).ToList();
            packageIdentities.Select(x =>
                {
                    var uri = File.Exists(Path.Combine(sourceRepository.PackageSource.Source, x.Id, $"{x.Id}.{x.Version}.nupkg"));
                    return uri;
                });
        }
    }
}
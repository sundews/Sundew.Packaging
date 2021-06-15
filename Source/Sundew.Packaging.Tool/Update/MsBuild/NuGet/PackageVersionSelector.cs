// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageVersionSelector.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Update.MsBuild.NuGet
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::NuGet.Versioning;
    using Sundew.Base.Collections;
    using Sundew.Packaging.Tool.RegularExpression;

    public class PackageVersionSelector
    {
        private readonly INuGetPackageVersionFetcher nuGetPackageVersionFetcher;
        private readonly IPackageVersionSelectorReporter? packageVersionSelectorReporter;

        private readonly Dictionary<string, IReadOnlyList<NuGetVersion>> cache = new();

        public PackageVersionSelector(INuGetPackageVersionFetcher nuGetPackageVersionFetcher, IPackageVersionSelectorReporter? packageVersionSelectorReporter)
        {
            this.nuGetPackageVersionFetcher = nuGetPackageVersionFetcher;
            this.packageVersionSelectorReporter = packageVersionSelectorReporter;
        }

        public async Task<IEnumerable<PackageUpdate>> GetPackageVersions(IReadOnlyList<PackageUpdateSuggestion> possiblePackageUpdates, GlobRegex? globalGlobRegex, string rootDirectory, bool allowPrerelease, string? source)
        {
            return (await possiblePackageUpdates.SelectAsync(async x =>
                {
                    var actualGlobRegex = x.GlobRegex ?? globalGlobRegex;
                    var newNuGetVersion = actualGlobRegex != null && !actualGlobRegex.IsPattern ? NuGetVersion.Parse(actualGlobRegex.Pattern) : await this.GetLatestVersion(x.Id, actualGlobRegex, rootDirectory, allowPrerelease, source);

                    if (x.NuGetVersion != newNuGetVersion)
                    {
                        this.packageVersionSelectorReporter?.PackageUpdateSelected(x.Id, x.NuGetVersion, newNuGetVersion);
                        return new PackageUpdate(x.Id, x.NuGetVersion, newNuGetVersion);
                    }

                    return default;
                }))
                .Where(x => x != null)
                .Cast<PackageUpdate>();
        }

        private async Task<NuGetVersion> GetLatestVersion(
            string packageId,
            GlobRegex? globRegex,
            string rootDirectory,
            bool allowPrerelease,
            string? source)
        {
            if (!this.cache.TryGetValue(packageId, out var versions))
            {
                versions = (await this.nuGetPackageVersionFetcher.GetAllVersionsAsync(rootDirectory, source, packageId))
                    .Distinct()
                    .OrderByDescending(x => x)
                    .ToList();
                this.cache.Add(packageId, versions);
            }

            if (globRegex != null)
            {
                return versions.FirstOrDefault(x =>
                    globRegex.IsMatch(x.ToFullString())
                    && (!x.IsPrerelease || (allowPrerelease && x.IsPrerelease))) ?? throw new NuGetVersionNotFoundException(packageId, globRegex.Glob, allowPrerelease, versions);
            }

            return versions.First(x => !x.IsPrerelease || (allowPrerelease && x.IsPrerelease));
        }
    }
}
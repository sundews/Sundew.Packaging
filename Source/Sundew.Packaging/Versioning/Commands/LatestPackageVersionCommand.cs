// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LatestPackageVersionCommand.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.Commands;

using System;
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

/// <summary>
/// Command to get the latest major minor version of a package.
/// </summary>
/// <seealso cref="Sundew.Packaging.Versioning.Commands.ILatestPackageVersionCommand" />
public class LatestPackageVersionCommand : ILatestPackageVersionCommand
{
    private const string Separator = ", ";
    private const string DeterminingLatestVersionFromSources = "Determining latest version from sources: ";
    private readonly ILogger nuGetLogger;
    private readonly Logging.ILogger? logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LatestPackageVersionCommand"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="nuGetLogger">The nu get logger.</param>
    public LatestPackageVersionCommand(Logging.ILogger? logger, ILogger nuGetLogger)
    {
        this.nuGetLogger = nuGetLogger;
        this.logger = logger;
    }

    /// <summary>
    /// Gets the latest major minor version.
    /// </summary>
    /// <param name="packageId">The package identifier.</param>
    /// <param name="sources">The sources.</param>
    /// <param name="nuGetVersion">The nu get version.</param>
    /// <param name="includePatchInMatch">if set to <c>true</c> [include patch in match].</param>
    /// <param name="allowPrerelease">if set to <c>true</c> [allow prerelease].</param>
    /// <returns>The version.</returns>
    public async Task<NuGetVersion?> GetLatestMajorMinorVersion(
        string packageId,
        IReadOnlyList<string> sources,
        NuGetVersion nuGetVersion,
        bool includePatchInMatch,
        bool allowPrerelease)
    {
        this.logger?.LogInfo(new StringBuilder(DeterminingLatestVersionFromSources).AppendItems(sources, Separator).ToString());
        var latestVersion = (await sources.SelectAsync(async sourceUri =>
            {
                try
                {
                    PackageSource packageSource = new(sourceUri);
                    var resourceAsync = await Repository.Factory.GetCoreV3(packageSource.Source)
                        .GetResourceAsync<FindPackageByIdResource>(CancellationToken.None).ConfigureAwait(false);
                    return await resourceAsync.GetAllVersionsAsync(
                        packageId,
                        new SourceCacheContext { NoCache = true, RefreshMemoryCache = true },
                        this.nuGetLogger,
                        CancellationToken.None).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return default;
                }
                catch (Exception e)
                {
                    this.logger?.LogMessage($"SPP: Failed to retrieve version from source: {sourceUri}, with exception: {e}");
                    return default;
                }
            }))
            .Where(x => x != default)
            .SelectMany(x => x)
            .OrderByDescending(x => x)
            .FirstOrDefault(x =>
                x.Major == nuGetVersion.Major
                && x.Minor == nuGetVersion.Minor
                && (x.Patch == nuGetVersion.Patch || !includePatchInMatch)
                && (!x.IsPrerelease || (allowPrerelease && x.IsPrerelease)));
        if (latestVersion != null)
        {
            this.logger?.LogInfo($"SPP: Found latest version: {latestVersion}");
        }
        else
        {
            this.logger?.LogInfo("SPP: No version found");
        }

        return latestVersion;
    }
}
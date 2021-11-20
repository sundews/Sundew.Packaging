// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageExistsCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.Commands;

using System;
using System.Threading;
using System.Threading.Tasks;
using global::NuGet.Common;
using global::NuGet.Configuration;
using global::NuGet.Protocol;
using global::NuGet.Protocol.Core.Types;
using global::NuGet.Versioning;

/// <summary>
/// A command that tests if a package exists.
/// </summary>
/// <seealso cref="Sundew.Packaging.Versioning.Commands.IPackageExistsCommand" />
public class PackageExistsCommand : IPackageExistsCommand
{
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackageExistsCommand"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public PackageExistsCommand(ILogger logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Existses the asynchronous.
    /// </summary>
    /// <param name="packageId">The package identifier.</param>
    /// <param name="semanticVersion">The semantic version.</param>
    /// <param name="sourceUri">The source URI.</param>
    /// <returns>An async task with a value indicating whether the package exists.</returns>
    /// <exception cref="System.NotSupportedException">Thrown when no NuGet resource could be found.</exception>
    public async Task<bool> ExistsAsync(string packageId, SemanticVersion semanticVersion, string sourceUri)
    {
        PackageSource packageSource = new(sourceUri);
        var resourceAsync = await Repository.Factory.GetCoreV3(packageSource.Source).GetResourceAsync<FindPackageByIdResource>(CancellationToken.None).ConfigureAwait(false);
        if (resourceAsync != null)
        {
            return await resourceAsync.DoesPackageExistAsync(
                packageId,
                new NuGetVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch, semanticVersion.Release),
                new NullSourceCacheContext(),
                this.logger,
                CancellationToken.None).ConfigureAwait(false);
        }

        throw new NotSupportedException($"{nameof(RemoteV3FindPackageByIdResource)} not supported.");
    }
}
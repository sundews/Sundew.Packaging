// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutomaticPackageVersioner.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Internal
{
    using System;
    using System.Threading.Tasks;
    using global::NuGet.Common;
    using global::NuGet.Versioning;
    using Sundew.Build.Publish.Internal.Commands;

    internal class AutomaticPackageVersioner : IAutomaticPackageVersioner
    {
        private readonly IPackageExistsCommand localPackageExistsCommand;
        private readonly IPackageExistsCommand remotePackageExistsCommand;

        public AutomaticPackageVersioner(IPackageExistsCommand localPackageExistsCommand, IPackageExistsCommand remotePackageExistsCommand)
        {
            this.localPackageExistsCommand = localPackageExistsCommand;
            this.remotePackageExistsCommand = remotePackageExistsCommand;
        }

        public async Task<SemanticVersion> GetSemanticVersion(string packageId, SemanticVersion semanticVersion, string sourceUri, ILogger logger)
        {
            var packageExistsCommand = this.localPackageExistsCommand;
            if (!UriUtility.TryCreateSourceUri(sourceUri, UriKind.Absolute).IsFile)
            {
                packageExistsCommand = this.remotePackageExistsCommand;
            }

            return new SemanticVersion(
                semanticVersion.Major,
                semanticVersion.Minor,
                semanticVersion.Patch + (await packageExistsCommand.ExistsAsync(packageId, semanticVersion, sourceUri, logger) ? 1 : 0));
        }
    }
}
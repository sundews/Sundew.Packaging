// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalPackageExistsCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Commands
{
    using System.IO;
    using System.Threading.Tasks;
    using NuGet.Common;
    using NuGet.Versioning;
    using Sundew.Build.Publish.Internal.Commands;
    using Sundew.Build.Publish.Internal.IO;

    internal class LocalPackageExistsCommand : IPackageExistsCommand
    {
        private readonly IFileSystem fileSystem;

        public LocalPackageExistsCommand(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public Task<bool> ExistsAsync(string packageId, SemanticVersion semanticVersion, string sourceUri, ILogger logger)
        {
            return Task.FromResult(this.fileSystem.FileExists(Path.Combine(sourceUri, $@"{packageId}\{packageId}.{semanticVersion.Major}.{semanticVersion.Minor}.{semanticVersion.Patch}.nupkg")));
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PaketDependenciesTemporarySourceInjector.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PaketLocalUpdate
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Paket;
    using Sundew.Packaging.Versioning.IO;

    /// <summary>
    /// Injects a local source into paket.dependencies and applies prerelease tag to the specified packages if necessary.
    /// Reverts the change upon disposal.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class PaketDependenciesTemporarySourceInjector : IDisposable
    {
        private const string Source = "source ";

        private readonly string backupDependencies;
        private readonly PaketDependenciesParser paketDependenciesParser;
        private readonly IFileSystemAsync fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaketDependenciesTemporarySourceInjector" /> class.
        /// </summary>
        /// <param name="dependencies">The dependencies.</param>
        /// <param name="paketDependenciesParser">The paket dependencies parser.</param>
        /// <param name="fileSystem">The file system.</param>
        public PaketDependenciesTemporarySourceInjector(Dependencies dependencies, PaketDependenciesParser paketDependenciesParser, IFileSystemAsync fileSystem)
        {
            this.Dependencies = dependencies;
            this.paketDependenciesParser = paketDependenciesParser;
            this.fileSystem = fileSystem;
            this.backupDependencies = dependencies.DependenciesFile + ".bak";
        }

        /// <summary>
        /// Gets the dependencies.
        /// </summary>
        /// <value>
        /// The dependencies.
        /// </value>
        public Dependencies Dependencies { get; }

        /// <summary>
        /// Injects the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="group">The group.</param>
        /// <returns>An async task.</returns>
        public async Task Inject(string source, string packageId, string group)
        {
            this.fileSystem.Copy(this.Dependencies.DependenciesFile, this.backupDependencies, true);
            var fileContent = await this.fileSystem.ReadAllTextAsync(this.Dependencies.DependenciesFile);
            var paketDependencies = this.paketDependenciesParser.Parse(fileContent);
            if (paketDependencies.TryGetValue(group, out var paketGroup))
            {
                var stringBuilder = new StringBuilder(fileContent);
                var packages = paketGroup.Packages.Where(x => Regex.IsMatch(x.Id, $"^{packageId}$"));
                foreach (var package in packages.Reverse())
                {
                    if (!package.IsPrerelease)
                    {
                        stringBuilder.Insert(package.PrereleaseIndex + package.PrereleaseLength, " prerelease");
                    }
                }

                var beginningOfFirstSource = paketGroup.Sources.FirstOrDefault().Index;

                if (beginningOfFirstSource > -1)
                {
                    stringBuilder.Insert(beginningOfFirstSource, $"{Source}{source}{Environment.NewLine}");
                }

                await this.fileSystem.WriteAllTextAsync(this.Dependencies.DependenciesFile, stringBuilder.ToString());
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.fileSystem.FileExists(this.backupDependencies))
            {
                this.fileSystem.Copy(this.backupDependencies, this.Dependencies.DependenciesFile, true);
            }
        }
    }
}
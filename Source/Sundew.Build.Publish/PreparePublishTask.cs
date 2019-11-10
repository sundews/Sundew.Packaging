// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PreparePublishTask.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish
{
    using System;
    using System.IO;
    using System.Reflection;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using NuGet.Versioning;
    using Sundew.Base.Enumerations;
    using Sundew.Base.Time;
    using Sundew.Build.Publish.Commands;
    using Sundew.Build.Publish.Internal;
    using Sundew.Build.Publish.Internal.NuGet.Configuration;

    /// <summary>MSBuild task that prepare for publishing the created NuGet package.</summary>
    /// <seealso cref="Microsoft.Build.Utilities.Task" />
    public class PreparePublishTask : Task
    {
        internal const string DefaultLocalSourceName = "Local (Sundew)";
        internal static readonly string LocalSourceBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetExecutingAssembly().GetName().Name);
        internal static readonly string DefaultLocalSource = Path.Combine(LocalSourceBasePath, "packages");
        private readonly IAddLocalSourceCommand addLocalSourceCommand;
        private readonly IPrereleaseVersioner prereleaseVersioner;

        /// <summary>Initializes a new instance of the <see cref="PreparePublishTask"/> class.</summary>
        public PreparePublishTask()
            : this(new AddLocalSourceCommand(new Internal.IO.FileSystem(), new SettingsFactory()), new PrereleaseVersioner(new DateTimeProvider()))
        {
        }

        internal PreparePublishTask(IAddLocalSourceCommand addLocalSourceCommand, IPrereleaseVersioner prereleaseVersioner)
        {
            this.addLocalSourceCommand = addLocalSourceCommand;
            this.prereleaseVersioner = prereleaseVersioner;
        }

        /// <summary>Gets or sets the solution dir.</summary>
        /// <value>The solution dir.</value>
        [Required]
        public string SolutionDir { get; set; }

        /// <summary>Gets or sets the version.</summary>
        /// <value>The version.</value>
        [Required]
        public string Version { get; set; }

        /// <summary>Gets or sets the prerelease versioning mode.</summary>
        /// <value>The prerelease versioning mode.</value>
        public string PrereleaseVersioningMode { get; set; }

        /// <summary>Gets or sets the name of the local source.</summary>
        /// <value>The name of the local source.</value>
        public string LocalSourceName { get; set; }

        /// <summary>Gets or sets the name of the source.
        /// This property is using to select between the various source.
        /// There are two supported hardcoded values:
        /// default: creates a prerelease and pushes it to the default push source.
        /// default-stable: creates a stable version and pushes it the default push source.</summary>
        /// <value>The name of the source.</value>
        public string SourceName { get; set; }

        /// <summary>Gets or sets the production source.
        /// The production source is a string in the following format:
        /// StageRegex|SourceUri|SymbolsSourceUri
        /// The StageRegex is a regex that will be matched against the SourceName property and if a match occurs this source will be used to push the package.
        /// The Source Uri is an uri of a NuGet server or local folder.
        /// </summary>
        /// <value>The production source.</value>
        public string ProductionSource { get; set; }

        /// <summary>Gets or sets the integration source.
        /// The integration source is a string in the following format:
        /// StageRegex|SourceUri|SymbolsSourceUri
        /// The StageRegex is a regex that will be matched against the SourceName property and if a match occurs this source will be used to push the package.
        /// The Source Uri is an uri of a NuGet server or local folder.
        /// </summary>
        /// <value>The integration source.</value>
        public string IntegrationSource { get; set; }

        /// <summary>Gets or sets the development source.
        /// The development source is a string in the following format:
        /// StageRegex|SourceUri|SymbolsSourceUri
        /// The StageRegex is a regex that will be matched against the SourceName property and if a match occurs this source will be used to push the package.
        /// The Source Uri is an uri of a NuGet server or local folder.
        /// </summary>
        /// <value>The development source.</value>
        public string DevelopmentSource { get; set; }

        /// <summary>Gets or sets the local source.</summary>
        /// <value>The local source.</value>
        public string LocalSource { get; set; }

        /// <summary>Gets the package version.</summary>
        /// <value>The package version.</value>
        [Output]
        public string PackageVersion { get; private set; }

        /// <summary>Gets the source.</summary>
        /// <value>The source.</value>
        [Output]
        public string Source { get; private set; }

        /// <summary>Gets the symbols source.</summary>
        /// <value>The symbols source.</value>
        [Output]
        public string SymbolsSource { get; private set; }

        /// <summary>Must be implemented by derived class.</summary>
        /// <returns>true, if successful.</returns>
        public override bool Execute()
        {
            var localSourceName = this.LocalSourceName ?? DefaultLocalSourceName;
            var localSource = this.addLocalSourceCommand.Add(this.SolutionDir, localSourceName, this.LocalSource ?? DefaultLocalSource);

            var pushSource = Internal.Source.SelectSource(
                this.SourceName,
                this.ProductionSource,
                this.IntegrationSource,
                this.DevelopmentSource,
                localSource.Path,
                localSource.DefaultSettings);

            this.Source = pushSource.Uri;
            this.SymbolsSource = pushSource.SymbolsUri;

            if (SemanticVersion.TryParse(this.Version, out var semanticVersion))
            {
                if (pushSource.IsRelease)
                {
                    this.PackageVersion = semanticVersion.ToFullString();
                }
                else
                {
                    if (!this.PrereleaseVersioningMode.TryParseEnum(out PrereleaseVersioningMode prereleaseVersioningMode, true))
                    {
                        prereleaseVersioningMode = Publish.PrereleaseVersioningMode.IncrementPatch;
                    }

                    this.PackageVersion = this.prereleaseVersioner.GetPrereleaseVersion(semanticVersion, prereleaseVersioningMode, pushSource).ToFullString();
                }

                return true;
            }

            return false;
        }
    }
}
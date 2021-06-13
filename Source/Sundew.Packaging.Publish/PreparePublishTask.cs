// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PreparePublishTask.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish
{
    using System;
    using System.IO;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using NuGet.Versioning;
    using Sundew.Base.Primitives;
    using Sundew.Base.Primitives.Time;
    using Sundew.Packaging.Publish.Internal;
    using Sundew.Packaging.Publish.Internal.Logging;
    using Sundew.Packaging.Source;
    using Sundew.Packaging.Staging;
    using Sundew.Packaging.Versioning;
    using Sundew.Packaging.Versioning.Commands;
    using Sundew.Packaging.Versioning.IO;
    using Sundew.Packaging.Versioning.Logging;
    using Sundew.Packaging.Versioning.NuGet.Configuration;
    using ILogger = Sundew.Packaging.Versioning.Logging.ILogger;

    /// <summary>MSBuild task that prepare for publishing the created NuGet package.</summary>
    /// <seealso cref="Microsoft.Build.Utilities.Task" />
    public class PreparePublishTask : Task
    {
        private readonly IFileSystem fileSystem;

        private readonly INuGetSettingsInitializationCommand nuGetSettingsInitializationCommand;

        private readonly IPackageVersioner packageVersioner;
        private readonly INuGetVersionProvider nuGetVersionProvider;
        private readonly ILatestVersionSourcesCommand latestVersionSourcesCommand;

        private readonly ILogger logger;
        private readonly NuGetToLoggerAdapter nuGetLogger;
        private readonly IPublishInfoProvider publishInfoProvider;
        private readonly PrereleaseDateTimeProvider prereleaseDateTimeProvider;

        /// <summary>Initializes a new instance of the <see cref="PreparePublishTask"/> class.</summary>
        public PreparePublishTask()
            : this(
                new SettingsFactory(),
                new FileSystem(),
                null,
                null,
                null,
                null)
        {
        }

        internal PreparePublishTask(
            ISettingsFactory settingsFactory,
            IFileSystem fileSystem,
            IPackageVersioner? packageVersioner,
            IDateTime? dateTime,
            INuGetVersionProvider? nuGetVersionProvider,
            ILogger? logger)
        {
            this.fileSystem = fileSystem;
            this.logger = logger ?? new MsBuildLogger(this.Log);
            this.nuGetLogger = new NuGetToLoggerAdapter(this.logger);
            this.nuGetSettingsInitializationCommand = new NuGetSettingsInitializationCommand(settingsFactory);
            this.publishInfoProvider = new PublishInfoProvider(this.fileSystem, this.logger);
            this.packageVersioner = packageVersioner ?? new PackageVersioner(new PackageExistsCommand(this.nuGetLogger), new LatestPackageVersionCommand(this.logger, this.nuGetLogger), this.logger);
            this.nuGetVersionProvider = nuGetVersionProvider ?? new NuGetVersionProvider(this.fileSystem, this.logger);
            this.prereleaseDateTimeProvider = new PrereleaseDateTimeProvider(this.fileSystem, dateTime ?? new DateTimeProvider(), this.logger);
            this.latestVersionSourcesCommand = new LatestVersionSourcesCommand(this.fileSystem);
        }

        /// <summary>Gets or sets the solution dir.</summary>
        /// <value>The solution dir.</value>
        [Required]
        public string? SolutionDir { get; set; }

        /// <summary>
        /// Gets or sets the build info file path.
        /// </summary>
        /// <value>
        /// The build info file path.
        /// </value>
        [Required]
        public string? BuildInfoFilePath { get; set; }

        /// <summary>
        /// Gets or sets the publish information file path.
        /// </summary>
        /// <value>
        /// The publish information file path.
        /// </value>
        [Required]
        public string? PublishInfoFilePath { get; set; }

        /// <summary>
        /// Gets or sets the version file path.
        /// </summary>
        /// <value>
        /// The version file path.
        /// </value>
        [Required]
        public string? VersionFilePath { get; set; }

        /// <summary>
        /// Gets or sets the referenced package version file path.
        /// </summary>
        /// <value>
        /// The version referenced package file path.
        /// </value>
        [Required]
        public string? ReferencedPackageVersionFilePath { get; set; }

        /// <summary>Gets or sets the package identifier.</summary>
        /// <value>The package identifier.</value>
        [Required]
        public string? PackageId { get; set; }

        /// <summary>Gets or sets the version.</summary>
        /// <value>The version.</value>
        [Required]
        public string? Version { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is source publish enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is source publish enabled; otherwise, <c>false</c>.
        /// </value>
        [Required]
        public bool IsSourcePublishEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether all should be added to latest version sources.
        /// </summary>
        /// <value>
        ///   <c>true</c> if all sources should be added to latest version sources; otherwise, <c>false</c>.
        /// </value>
        public bool AddAllSourcesToLatestVersionSources { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether NuGetOrg should be added to latest version sources.
        /// </summary>
        /// <value>
        ///   <c>true</c> if NuGetOrg should be added to latest version sources; otherwise, <c>false</c>.
        /// </value>
        public bool AddNuGetOrgSourceToLatestVersionSources { get; set; } = true;

        /// <summary>Gets or sets the versioning mode.</summary>
        /// <value>The versioning mode.</value>
        public string? VersioningMode { get; set; }

        /// <summary>Gets or sets the name of the local source.</summary>
        /// <value>The name of the local source.</value>
        public string? LocalSourceName { get; set; }

        /// <summary>Gets or sets the name of the source.
        /// This property is using to select between the various source.
        /// There are two supported hardcoded values:
        /// default: creates a prerelease and pushes it to the default push source.
        /// default-stable: creates a stable version and pushes it the default push source.</summary>
        /// <value>The name of the source.</value>
        public string? Stage { get; set; }

        /// <summary>Gets or sets the production source.
        /// The production source is a string in the following format:
        /// StageRegex[>StageName]|SourceUri[|SymbolsSourceUri]
        /// The StageRegex is a regex that will be matched against the SourceName property and if a match occurs this source will be used to push the package.
        /// The Source Uri is an uri of a NuGet server or local folder.
        /// </summary>
        /// <value>The production source.</value>
        public string? Production { get; set; }

        /// <summary>Gets or sets the integration source.
        /// The integration source is a string in the following format:
        /// StageRegex[>StageName]|SourceUri[|SymbolsSourceUri]
        /// The StageRegex is a regex that will be matched against the SourceName property and if a match occurs this source will be used to push the package.
        /// The Source Uri is an uri of a NuGet server or local folder.
        /// </summary>
        /// <value>The integration source.</value>
        public string? Integration { get; set; }

        /// <summary>Gets or sets the development source.
        /// The development source is a string in the following format:
        /// StageRegex[>StageName]|SourceUri[|SymbolsSourceUri]
        /// The StageRegex is a regex that will be matched against the SourceName property and if a match occurs this source will be used to push the package.
        /// The Source Uri is an uri of a NuGet server or local folder.
        /// </summary>
        /// <value>The development source.</value>
        public string? Development { get; set; }

        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        /// <value>
        /// The API key.
        /// </value>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the symbols API key.
        /// </summary>
        /// <value>
        /// The API key.
        /// </value>
        public string? SymbolsApiKey { get; set; }

        /// <summary>Gets or sets the local source.</summary>
        /// <value>The local source.</value>
        public string? LocalSource { get; set; }

        /// <summary>
        /// Gets or sets the local package stage.
        /// </summary>
        /// <value>
        /// The local package stage.
        /// </value>
        public string? LocalPackageStage { get; set; }

        /// <summary>
        /// Gets or sets the prerelease prefix.
        /// </summary>
        /// <value>
        /// The prerelease prefix.
        /// </value>
        public string? PrereleasePrefix { get; set; }

        /// <summary>
        /// Gets or sets the prerelease postfix.
        /// </summary>
        /// <value>
        /// The prerelease postfix.
        /// </value>
        public string? PrereleasePostfix { get; set; }

        /// <summary>Gets or sets a value indicating whether [allow local source].</summary>
        /// <value>
        ///   <c>true</c> if [allow local source]; otherwise, <c>false</c>.</value>
        public bool AllowLocalSource { get; set; }

        /// <summary>
        /// Gets or sets the latest version sources.
        /// Multiple sources must be specified with the pipe (|) character.
        /// </summary>
        /// <value>
        /// The get latest version sources.
        /// </value>
        public string? LatestVersionSources { get; set; }

        /// <summary>
        /// Gets or sets the prerelease format.
        /// </summary>
        /// <value>
        /// The prerelease format.
        /// </value>
        public string? PrereleaseFormat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include symbols].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include symbols]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeSymbols { get; set; }

        /// <summary>Gets or sets the metadata.</summary>
        /// <value>The metadata.</value>
        public string? Metadata { get; set; }

        /// <summary>Gets or sets the metadata format.</summary>
        /// <value>The metadata format.</value>
        public string? MetadataFormat { get; set; }

        /// <summary>Gets or sets the parameter.</summary>
        /// <value>The parameter.</value>
        public string? Parameter { get; set; }

        /// <summary>
        /// Gets or sets the version format.
        /// </summary>
        /// <value>
        /// The version format.
        /// </value>
        public string? VersionFormat { get; set; }

        /// <summary>
        /// Gets or sets the forced version.
        /// </summary>
        /// <value>
        /// The forced version.
        /// </value>
        public string? ForceVersion { get; set; }

        /// <summary>
        /// Gets the package version.
        /// </summary>
        /// <value>
        /// The package version.
        /// </value>
        [Output]
        public string? PackageVersion { get; private set; }

        internal PublishInfo? PublishInfo { get; private set; }

        /// <summary>Must be implemented by derived class.</summary>
        /// <returns>true, if successful.</returns>
        public override bool Execute()
        {
            try
            {
                var workingDirectory = WorkingDirectorySelector.GetWorkingDirectory(this.SolutionDir, this.fileSystem);
                var publishInfoFilePath = this.PublishInfoFilePath ?? throw new ArgumentNullException(nameof(this.PublishInfoFilePath), $"{nameof(this.PublishInfoFilePath)} was not initialized.");
                var versionFilePath = Path.Combine(workingDirectory, this.VersionFilePath ?? throw new ArgumentNullException(nameof(this.VersionFilePath), $"{nameof(this.VersionFilePath)} was not initialized."));
                var referencedPackageVersionFilePath = this.ReferencedPackageVersionFilePath ?? throw new ArgumentNullException(nameof(this.ReferencedPackageVersionFilePath), $"{nameof(this.ReferencedPackageVersionFilePath)} was not initialized.");
                var buildDateTimeFilePath = Path.Combine(workingDirectory, this.BuildInfoFilePath ?? throw new ArgumentNullException(nameof(this.BuildInfoFilePath), $"{nameof(this.BuildInfoFilePath)} was not initialized."));

                if (this.fileSystem.FileExists(publishInfoFilePath) && this.nuGetVersionProvider.Read(versionFilePath, out var version))
                {
                    this.PackageVersion = version;
                    return true;
                }

                var localSourceName = this.LocalSourceName ?? PackageSources.DefaultLocalSourceName;
                var nuGetSettings = this.nuGetSettingsInitializationCommand.Initialize(workingDirectory, localSourceName, this.LocalSource ?? PackageSources.DefaultLocalSource);

                var selectedSource = StageSelector.Select(
                    this.Stage,
                    this.Production,
                    this.Integration,
                    this.Development,
                    nuGetSettings.LocalSourcePath,
                    this.PrereleaseFormat,
                    this.ApiKey,
                    this.SymbolsApiKey,
                    this.LocalPackageStage,
                    this.PrereleasePrefix,
                    this.PrereleasePostfix,
                    nuGetSettings.DefaultSettings,
                    this.AllowLocalSource,
                    this.IsSourcePublishEnabled);

                var latestVersionSources =
                    this.latestVersionSourcesCommand.GetLatestVersionSources(
                        this.LatestVersionSources,
                        selectedSource,
                        nuGetSettings,
                        this.AddNuGetOrgSourceToLatestVersionSources,
                        this.AddAllSourcesToLatestVersionSources);

                if (NuGetVersion.TryParse(this.Version, out var nuGetVersion))
                {
                    var versioningMode = Versioning.VersioningMode.AutomaticLatestPatch;
                    this.VersioningMode?.TryParseEnum(out versioningMode, true);
                    var buildDateTime = this.prereleaseDateTimeProvider.GetBuildDateTime(buildDateTimeFilePath);
                    var packageVersion = this.packageVersioner.GetVersion(this.PackageId!, nuGetVersion, this.VersionFormat, this.ForceVersion, versioningMode, selectedSource, latestVersionSources, buildDateTime, this.Metadata, this.MetadataFormat, this.Parameter ?? string.Empty);
                    var fullPackageVersion = packageVersion.ToFullString();
                    this.PublishInfo = this.publishInfoProvider.Save(publishInfoFilePath, selectedSource, packageVersion.ToNormalizedString(), packageVersion.ToFullString(), packageVersion.Metadata, this.IncludeSymbols);
                    this.nuGetVersionProvider.Save(versionFilePath, referencedPackageVersionFilePath, fullPackageVersion);
                    this.PackageVersion = fullPackageVersion;
                    return true;
                }

                this.logger.LogError($"Could not parse package version: {this.Version}");
                return false;
            }
            catch (Exception e)
            {
                this.logger.LogError(e.ToString());
                return false;
            }
        }
    }
}
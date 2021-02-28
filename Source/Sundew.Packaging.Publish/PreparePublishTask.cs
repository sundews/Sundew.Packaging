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
    using System.Reflection;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using NuGet.Versioning;
    using Sundew.Base.Enumerations;
    using Sundew.Base.Time;
    using Sundew.Packaging.Publish.Internal;
    using Sundew.Packaging.Publish.Internal.Commands;
    using Sundew.Packaging.Publish.Internal.IO;
    using Sundew.Packaging.Publish.Internal.Logging;
    using Sundew.Packaging.Publish.Internal.NuGet.Configuration;

    /// <summary>MSBuild task that prepare for publishing the created NuGet package.</summary>
    /// <seealso cref="Microsoft.Build.Utilities.Task" />
    public class PreparePublishTask : Task
    {
        internal const string DefaultLocalSourceName = "Local-Sundew";
        internal static readonly string LocalSourceBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), GetFolderName(Assembly.GetExecutingAssembly().GetName().Name));
        internal static readonly string DefaultLocalSource = Path.Combine(LocalSourceBasePath, "packages");
        private const string MergedAssemblyEnding = ".m";

        private readonly ISettingsFactory settingsFactory;

        private readonly IFileSystem fileSystem;

        private readonly INuGetSettingsInitializationCommand nuGetSettingsInitializationCommand;

        private readonly IPackageVersioner packageVersioner;
        private readonly ILatestVersionSourcesCommand latestVersionSourcesCommand;

        private readonly ICommandLogger commandLogger;

        /// <summary>Initializes a new instance of the <see cref="PreparePublishTask"/> class.</summary>
        public PreparePublishTask()
            : this(
                new SettingsFactory(),
                new FileSystem(),
                new PackageVersioner(
                    new DateTimeProvider(),
                    new PackageExistsCommand(),
                    new LatestPackageVersionCommand()),
                null)
        {
        }

        internal PreparePublishTask(
            ISettingsFactory settingsFactory,
            IFileSystem fileSystem,
            IPackageVersioner packageVersioner,
            ICommandLogger? commandLogger)
        {
            this.settingsFactory = settingsFactory;
            this.fileSystem = fileSystem;
            this.nuGetSettingsInitializationCommand = new NuGetSettingsInitializationCommand(this.fileSystem, this.settingsFactory);
            this.packageVersioner = packageVersioner;
            this.latestVersionSourcesCommand = new LatestVersionSourcesCommand(this.fileSystem);
            this.commandLogger = commandLogger ?? new MsBuildCommandLogger(this.Log);
        }

        /// <summary>Gets or sets the solution dir.</summary>
        /// <value>The solution dir.</value>
        [Required]
        public string? SolutionDir { get; set; }

        /// <summary>Gets or sets the package identifier.</summary>
        /// <value>The package identifier.</value>
        [Required]
        public string? PackageId { get; set; }

        /// <summary>Gets or sets the version.</summary>
        /// <value>The version.</value>
        [Required]
        public string? Version { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow default push source for getting latest version].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow default push source for getting latest version]; otherwise, <c>false</c>.
        /// </value>
        public bool AddDefaultPushSourceToLatestVersionSources { get; set; } = true;

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
        public string? SourceName { get; set; }

        /// <summary>Gets or sets the production source.
        /// The production source is a string in the following format:
        /// StageRegex[>StageName]|SourceUri[|SymbolsSourceUri]
        /// The StageRegex is a regex that will be matched against the SourceName property and if a match occurs this source will be used to push the package.
        /// The Source Uri is an uri of a NuGet server or local folder.
        /// </summary>
        /// <value>The production source.</value>
        public string? ProductionSource { get; set; }

        /// <summary>Gets or sets the integration source.
        /// The integration source is a string in the following format:
        /// StageRegex[>StageName]|SourceUri[|SymbolsSourceUri]
        /// The StageRegex is a regex that will be matched against the SourceName property and if a match occurs this source will be used to push the package.
        /// The Source Uri is an uri of a NuGet server or local folder.
        /// </summary>
        /// <value>The integration source.</value>
        public string? IntegrationSource { get; set; }

        /// <summary>Gets or sets the development source.
        /// The development source is a string in the following format:
        /// StageRegex[>StageName]|SourceUri[|SymbolsSourceUri]
        /// The StageRegex is a regex that will be matched against the SourceName property and if a match occurs this source will be used to push the package.
        /// The Source Uri is an uri of a NuGet server or local folder.
        /// </summary>
        /// <value>The development source.</value>
        public string? DevelopmentSource { get; set; }

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
        /// Gets the working directory.
        /// </summary>
        /// <value>
        /// The working directory.
        /// </value>
        [Output]
        public string? WorkingDirectory { get; private set; }

        /// <summary>Gets the package version.</summary>
        /// <value>The package version.</value>
        [Output]
        public string? PackageVersion { get; private set; }

        /// <summary>Gets the source.</summary>
        /// <value>The source.</value>
        [Output]
        public string? Source { get; private set; }

        /// <summary>Gets the symbols source.</summary>
        /// <value>The symbols source.</value>
        [Output]
        public string? SymbolsSource { get; private set; }

        /// <summary>Gets a value indicating whether this instance is publish enabled.</summary>
        /// <value>
        ///   <c>true</c> if this instance is publish enabled; otherwise, <c>false</c>.</value>
        [Output]
        public bool PublishPackages { get; private set; }

        /// <summary>
        /// Gets the API key.
        /// </summary>
        /// <value>
        /// The API key.
        /// </value>
        [Output]
        public string? SourceApiKey { get; private set; }

        /// <summary>
        /// Gets the symbols source API key.
        /// </summary>
        /// <value>
        /// The symbols source API key.
        /// </value>
        [Output]
        public string? SymbolsSourceApiKey { get; private set; }

        /// <summary>Must be implemented by derived class.</summary>
        /// <returns>true, if successful.</returns>
        public override bool Execute()
        {
            this.WorkingDirectory = WorkingDirectorySelector.GetWorkingDirectory(this.SolutionDir, this.fileSystem);
            var localSourceName = this.LocalSourceName ?? DefaultLocalSourceName;
            var nuGetSettings = this.nuGetSettingsInitializationCommand.Initialize(this.WorkingDirectory, localSourceName, this.LocalSource ?? DefaultLocalSource);

            var source = SourceSelector.SelectSource(
                this.SourceName,
                this.ProductionSource,
                this.IntegrationSource,
                this.DevelopmentSource,
                nuGetSettings.LocalSourcePath,
                nuGetSettings.DefaultSettings,
                this.AllowLocalSource);

            var latestVersionSources =
                this.latestVersionSourcesCommand.GetLatestVersionSources(this.LatestVersionSources, source, nuGetSettings, this.AddDefaultPushSourceToLatestVersionSources);

            this.PublishPackages = source.IsEnabled;
            this.Source = source.Uri;
            this.SourceApiKey = this.GetApiKey(source);
            this.SymbolsSource = source.SymbolsUri;
            this.SymbolsSourceApiKey = this.GetSymbolsApiKey(source);
            if (NuGetVersion.TryParse(this.Version, out var nuGetVersion))
            {
                var versioningMode = Publish.VersioningMode.AutomaticLatestPatch;
                this.VersioningMode?.TryParseEnum(out versioningMode, true);
                this.PackageVersion = this.packageVersioner.GetVersion(this.PackageId!, nuGetVersion, versioningMode, source.IsStableRelease, source, latestVersionSources, new NuGetToMsBuildLoggerAdapter(this.Log)).ToFullString();

                return true;
            }

            this.commandLogger.LogError($"Could not parse package version: {this.Version}");
            return false;
        }

        private static string GetFolderName(string name)
        {
            if (name.EndsWith(MergedAssemblyEnding))
            {
                return name.Substring(0, name.Length - MergedAssemblyEnding.Length);
            }

            return name;
        }

        private string? GetApiKey(Source source)
        {
            if (source.ApiKey == string.Empty)
            {
                return null;
            }

            return source.ApiKey ?? this.ApiKey;
        }

        private string? GetSymbolsApiKey(Source source)
        {
            if (source.SymbolsApiKey == string.Empty)
            {
                return null;
            }

            return source.SymbolsApiKey ?? this.SymbolsApiKey ?? source.ApiKey ?? this.ApiKey;
        }
    }
}
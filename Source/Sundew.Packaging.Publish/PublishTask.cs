// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublishTask.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using NuGet.Common;
    using Sundew.Packaging.Publish.Internal;
    using Sundew.Packaging.Publish.Internal.Commands;
    using Sundew.Packaging.Publish.Internal.IO;
    using Sundew.Packaging.Publish.Internal.Logging;
    using Sundew.Packaging.Publish.Internal.NuGet.Configuration;
    using ILogger = Sundew.Packaging.Publish.Internal.Logging.ILogger;

    /// <summary>Publishes the created NuGet package to a specified package source.</summary>
    /// <seealso cref="Microsoft.Build.Utilities.Task" />
    public class PublishTask : Task
    {
        private const string PackageSourceText = "PackageSource";
        private const string PublishedText = "Published";
        private const string IsSymbolText = "IsSymbol";
        private const string SymbolsNupkgFileExtension = ".symbols.nupkg";
        private const string SnupkgFileExtension = ".snupkg";
        private const string NupkgFileExtension = ".nupkg";
        private const string PackagePathPackagePathDoesNotExistFormat = "The package path: {0} does not exist.";
        private const string PdbFileExtension = ".pdb";

        private readonly IPublishInfoProvider publishInfoProvider;
        private readonly IPushPackageCommand pushPackageCommand;
        private readonly ICopyPackageToLocalSourceCommand copyPackageToLocalSourceCommand;
        private readonly ICopyPdbToSymbolCacheCommand copyPdbToSymbolCacheCommand;
        private readonly IFileSystem fileSystem;
        private readonly ISettingsFactory settingsFactory;
        private readonly ILogger logger;
        private readonly IAppendPublishFileLogCommand appendPublishFileLogCommand;
        private readonly PruneSimilarPackageVersionsCommand pruneSimilarPackageVersionsCommand;

        /// <summary>Initializes a new instance of the <see cref="PublishTask"/> class.</summary>
        public PublishTask()
         : this(
             new Internal.IO.FileSystem(),
             null,
             new PushPackageCommand(),
             new CopyPackageToLocalSourceCommand(),
             new CopyPdbToSymbolCacheCommand(),
             new SettingsFactory(),
             null)
        {
        }

        internal PublishTask(
            IFileSystem fileSystem,
            IPublishInfoProvider? publishInfoProvider,
            IPushPackageCommand pushPackageCommand,
            ICopyPackageToLocalSourceCommand copyPackageToLocalSourceCommand,
            ICopyPdbToSymbolCacheCommand copyPdbToSymbolCacheCommand,
            ISettingsFactory settingsFactory,
            ILogger? commandLogger)
        {
            this.fileSystem = fileSystem;
            this.logger = commandLogger ?? new MsBuildLogger(this.Log);
            this.publishInfoProvider = publishInfoProvider ?? new PublishInfoProvider(this.fileSystem, this.logger);
            this.pushPackageCommand = pushPackageCommand;
            this.copyPackageToLocalSourceCommand = copyPackageToLocalSourceCommand;
            this.copyPdbToSymbolCacheCommand = copyPdbToSymbolCacheCommand;
            this.settingsFactory = settingsFactory;
            this.pruneSimilarPackageVersionsCommand = new PruneSimilarPackageVersionsCommand(this.fileSystem);
            this.appendPublishFileLogCommand = new AppendPublishFileLogCommand(this.fileSystem);
        }

        /// <summary>Gets or sets the solution dir.</summary>
        /// <value>The solution dir.</value>
        [Required]
        public string? SolutionDir { get; set; }

        /// <summary>
        /// Gets or sets the publish information file path.
        /// </summary>
        /// <value>
        /// The publish information file path.
        /// </value>
        [Required]
        public string? PublishInfoFilePath { get; set; }

        /// <summary>Gets or sets the project dir.</summary>
        /// <value>The project dir.</value>
        [Required]
        public string? ProjectDir { get; set; }

        /// <summary>Gets or sets the pack inputs.</summary>
        /// <value>The pack inputs.</value>
        [Required]
        public ITaskItem[]? PackInputs { get; set; }

        /// <summary>Gets or sets the output path.</summary>
        /// <value>The output path.</value>
        [Required]
        public string? OutputPath { get; set; }

        /// <summary>Gets or sets the package output path.</summary>
        /// <value>The package output path.</value>
        [Required]
        public string? PackageOutputPath { get; set; }

        /// <summary>Gets or sets the package identifier.</summary>
        /// <value>The package identifier.</value>
        [Required]
        public string? PackageId { get; set; }

        /// <summary>Gets or sets a value indicating whether [allow local source].</summary>
        /// <value>
        ///   <c>true</c> if [allow local source]; otherwise, <c>false</c>.</value>
        public bool AllowLocalSource { get; set; }

        /// <summary>Gets or sets a value indicating whether [copy PDB to symbol cache].</summary>
        /// <value>
        ///   <c>true</c> if [copy PDB to symbol cache]; otherwise, <c>false</c>.</value>
        public bool CopyLocalSourcePdbToSymbolCache { get; set; }

        /// <summary>Gets or sets the symbol cache dir.</summary>
        /// <value>The symbol cache dir.</value>
        public string? SymbolCacheDir { get; set; }

        /// <summary>Gets or sets a value indicating whether [no service endpoint].</summary>
        /// <value>
        ///   <c>true</c> if [no service endpoint]; otherwise, <c>false</c>.</value>
        public bool NoServiceEndpoint { get; set; }

        /// <summary>Gets or sets a value indicating whether [skip duplicate].</summary>
        /// <value>
        ///   <c>true</c> if [skip duplicate]; otherwise, <c>false</c>.</value>
        public bool SkipDuplicate { get; set; }

        /// <summary>Gets or sets the timeout in seconds.</summary>
        /// <value>The timeout in seconds.</value>
        public int TimeoutInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the publish log formats.
        /// </summary>
        /// <value>
        /// The publish log formats.
        /// </value>
        public string? PublishLogFormats { get; set; }

        /// <summary>
        /// Gets or sets the publish file log formats.
        /// </summary>
        /// <value>
        /// The publish file log formats.
        /// </value>
        public string? AppendPublishFileLogFormats { get; set; }

        /// <summary>Gets or sets the parameter.</summary>
        /// <value>The parameter.</value>
        public string? Parameter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [prune similar package versions].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [prune similar package versions]; otherwise, <c>false</c>.
        /// </value>
        public bool PruneSimilarPackageVersions { get; set; }

        /// <summary>Gets the package paths.</summary>
        /// <value>The package paths.</value>
        [Output]
        public ITaskItem[]? PackagePaths { get; private set; }

        /// <summary>Must be implemented by derived class.</summary>
        /// <returns>true, if successful.</returns>
        /// <exception cref="FileNotFoundException">The package path: {packagePath} does not exist.</exception>
        public override bool Execute()
        {
            var workingDirectory = WorkingDirectorySelector.GetWorkingDirectory(this.SolutionDir, this.fileSystem);
            var publishInfoFilePath = this.PublishInfoFilePath ?? throw new ArgumentNullException(nameof(this.PublishInfoFilePath), $"{nameof(this.PublishInfoFilePath)} was not set.");
            var packageId = this.PackageId ?? throw new ArgumentNullException(nameof(this.PackageId), $"{nameof(this.PackageId)} was not set.");

            try
            {
                var publishInfo = this.publishInfoProvider.Read(publishInfoFilePath);

                var packagePathWithoutExtension = this.GetPackagePathWithoutExtension(publishInfo.Version);
                var packagePath = packagePathWithoutExtension + NupkgFileExtension;
                if (!this.fileSystem.FileExists(packagePath))
                {
                    throw new FileNotFoundException(string.Format(PackagePathPackagePathDoesNotExistFormat, packagePath));
                }

                var symbolPackagePath = packagePathWithoutExtension + SnupkgFileExtension;
                if (!this.fileSystem.FileExists(symbolPackagePath))
                {
                    symbolPackagePath = packagePathWithoutExtension + SymbolsNupkgFileExtension;
                    if (!this.fileSystem.FileExists(symbolPackagePath))
                    {
                        symbolPackagePath = null;
                    }
                }

                var source = publishInfo.PushSource;
                var isValidSource = !string.IsNullOrEmpty(source);
                var isLocalSource = isValidSource && UriUtility.TryCreateSourceUri(source, UriKind.Absolute).IsFile;
                if (isValidSource && publishInfo.IsEnabled)
                {
                    var settings = this.settingsFactory.LoadDefaultSettings(workingDirectory);
                    if (isLocalSource)
                    {
                        this.copyPackageToLocalSourceCommand.Add(packageId, packagePath, source, this.SkipDuplicate, this.logger);
                        if (this.CopyLocalSourcePdbToSymbolCache)
                        {
                            this.copyPdbToSymbolCacheCommand.AddAndCleanCache(
                                this.PackInputs!.Where(x => Path.GetExtension(x.ItemSpec) == PdbFileExtension)
                                    .Select(x => x.ItemSpec).ToList(),
                                this.SymbolCacheDir,
                                settings,
                                this.logger);
                        }
                    }
                    else
                    {
                        this.pushPackageCommand.PushAsync(
                            packagePath,
                            source,
                            publishInfo.ApiKey,
                            symbolPackagePath,
                            publishInfo.SymbolsPushSource,
                            publishInfo.SymbolsApiKey,
                            this.TimeoutInSeconds,
                            settings,
                            this.NoServiceEndpoint,
                            this.SkipDuplicate,
                            new NuGetToMsBuildLoggerAdapter(this.logger),
                            this.logger).Wait();
                    }
                }

                if (isValidSource && this.PublishLogFormats != null && (!isLocalSource || this.AllowLocalSource))
                {
                    PublishLogger.Log(this.logger, this.PublishLogFormats, packageId, packagePath, symbolPackagePath, publishInfo, this.Parameter ?? string.Empty);
                }

                if (isValidSource && this.AppendPublishFileLogFormats != null && (!isLocalSource || this.AllowLocalSource))
                {
                    this.appendPublishFileLogCommand.Append(workingDirectory, this.AppendPublishFileLogFormats, packageId, packagePath, symbolPackagePath, publishInfo, this.Parameter ?? string.Empty, this.logger);
                }

                var packagePathTaskItem = new TaskItem(packagePath);
                packagePathTaskItem.SetMetadata(PackageSourceText, publishInfo.PushSource);
                packagePathTaskItem.SetMetadata(PublishedText, publishInfo.IsEnabled.ToString(CultureInfo.InvariantCulture));
                packagePathTaskItem.SetMetadata(IsSymbolText, false.ToString(CultureInfo.InvariantCulture));
                this.PackagePaths = new ITaskItem[symbolPackagePath != null ? 2 : 1];
                this.PackagePaths[0] = packagePathTaskItem;
                if (symbolPackagePath != null)
                {
                    var symbolsPackagePath = new TaskItem(symbolPackagePath);
                    symbolsPackagePath.SetMetadata(PackageSourceText, publishInfo.SymbolsPushSource);
                    symbolsPackagePath.SetMetadata(PublishedText, publishInfo.IsEnabled.ToString(CultureInfo.InvariantCulture));
                    symbolsPackagePath.SetMetadata(IsSymbolText, true.ToString(CultureInfo.InvariantCulture));
                    this.PackagePaths[1] = symbolsPackagePath;
                }

                if (this.PruneSimilarPackageVersions)
                {
                    this.pruneSimilarPackageVersionsCommand.Prune(packagePath, packageId, publishInfo.Version);
                }

                return true;
            }
            catch (Exception e)
            {
                this.logger.LogError(e.ToString());
                return false;
            }
            finally
            {
                this.publishInfoProvider.Delete(publishInfoFilePath);
            }
        }

        private string GetPackagePathWithoutExtension(string version)
        {
            return Path.Combine(this.ProjectDir!, this.PackageOutputPath!, $"{this.PackageId}.{version}");
        }
    }
}
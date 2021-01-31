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
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using NuGet.Common;
    using Sundew.Packaging.Publish.Internal;
    using Sundew.Packaging.Publish.Internal.Commands;
    using Sundew.Packaging.Publish.Internal.IO;
    using Sundew.Packaging.Publish.Internal.Logging;
    using Sundew.Packaging.Publish.Internal.NuGet.Configuration;

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
        private const string UndefinedText = "*Undefined*";

        private readonly IPushPackageCommand pushPackageCommand;
        private readonly ICopyPackageToLocalSourceCommand copyPackageToLocalSourceCommand;
        private readonly ICopyPdbToSymbolCacheCommand copyPdbToSymbolCacheCommand;
        private readonly IFileSystem fileSystem;
        private readonly ISettingsFactory settingsFactory;
        private readonly IPersistNuGetVersionCommand persistNuGetVersionCommand;
        private readonly ICommandLogger commandLogger;

        /// <summary>Initializes a new instance of the <see cref="PublishTask"/> class.</summary>
        public PublishTask()
         : this(
             new PushPackageCommand(),
             new CopyPackageToLocalSourceCommand(),
             new CopyPdbToSymbolCacheCommand(),
             new Internal.IO.FileSystem(),
             new SettingsFactory(),
             new PersistNuGetVersionCommand(new FileSystem()),
             null)
        {
        }

        internal PublishTask(
            IPushPackageCommand pushPackageCommand,
            ICopyPackageToLocalSourceCommand copyPackageToLocalSourceCommand,
            ICopyPdbToSymbolCacheCommand copyPdbToSymbolCacheCommand,
            IFileSystem fileSystem,
            ISettingsFactory settingsFactory,
            IPersistNuGetVersionCommand persistNuGetVersionCommand,
            ICommandLogger? commandLogger)
        {
            this.pushPackageCommand = pushPackageCommand;
            this.copyPackageToLocalSourceCommand = copyPackageToLocalSourceCommand;
            this.copyPdbToSymbolCacheCommand = copyPdbToSymbolCacheCommand;
            this.fileSystem = fileSystem;
            this.settingsFactory = settingsFactory;
            this.persistNuGetVersionCommand = persistNuGetVersionCommand;
            this.commandLogger = commandLogger ?? new MsBuildCommandLogger(this.Log);
        }

        /// <summary>Gets or sets the solution dir.</summary>
        /// <value>The solution dir.</value>
        [Required]
        public string? SolutionDir { get; set; }

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

        /// <summary>Gets or sets the assembly name.</summary>
        /// <value>The assembly name.</value>
        [Required]
        public string? AssemblyName { get; set; }

        /// <summary>Gets or sets the version.</summary>
        /// <value>The version.</value>
        [Required]
        public string? Version { get; set; }

        /// <summary>Gets or sets the source.</summary>
        /// <value>The source.</value>
        [Required]
        public string? Source { get; set; }

        /// <summary>Gets or sets the symbols source.</summary>
        /// <value>The symbols source.</value>
        public string? SymbolsSource { get; set; }

        /// <summary>Gets or sets the API key.</summary>
        /// <value>The API key.</value>
        public string? ApiKey { get; set; }

        /// <summary>Gets or sets the symbol API key.</summary>
        /// <value>The symbol API key.</value>
        public string? SymbolApiKey { get; set; }

        /// <summary>Gets or sets a value indicating whether this instance is publish enabled.</summary>
        /// <value>
        ///   <c>true</c> if this instance is publish enabled; otherwise, <c>false</c>.</value>
        public bool PublishPackages { get; set; }

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

        /// <summary>Gets the package paths.</summary>
        /// <value>The package paths.</value>
        [Output]
        public ITaskItem[]? PackagePaths { get; private set; }

        /// <summary>Must be implemented by derived class.</summary>
        /// <returns>true, if successful.</returns>
        /// <exception cref="FileNotFoundException">The package path: {packagePath} does not exist.</exception>
        public override bool Execute()
        {
            var packagePathWithoutExtension = this.GetPackagePathWithoutExtension();
            var packagePath = packagePathWithoutExtension + NupkgFileExtension;
            if (!this.fileSystem.FileExists(packagePath))
            {
                throw new FileNotFoundException(string.Format(PackagePathPackagePathDoesNotExistFormat, packagePath));
            }

            this.persistNuGetVersionCommand.Save(this.Version!, this.OutputPath!, this.AssemblyName!, this.commandLogger);
            var symbolPackagePath = packagePathWithoutExtension + SnupkgFileExtension;
            if (!this.fileSystem.FileExists(symbolPackagePath))
            {
                symbolPackagePath = packagePathWithoutExtension + SymbolsNupkgFileExtension;
                if (!this.fileSystem.FileExists(symbolPackagePath))
                {
                    symbolPackagePath = null;
                }
            }

            var source = this.Source;
            var isLocalSource = source != null && UriUtility.TryCreateSourceUri(source, UriKind.Absolute).IsFile;
            if (this.PublishPackages)
            {
                var workingDirectory = this.SolutionDir;
                if (workingDirectory == UndefinedText)
                {
                    workingDirectory = Path.GetDirectoryName(this.fileSystem.GetCurrentDirectory());
                }

                if (workingDirectory == null)
                {
                    throw new ArgumentException("The working directory cannot be null.", nameof(workingDirectory));
                }

                var settings = this.settingsFactory.LoadDefaultSettings(workingDirectory);
                if (isLocalSource)
                {
                    this.copyPackageToLocalSourceCommand.Add(this.PackageId!, packagePath, source!, this.SkipDuplicate, this.commandLogger);
                    if (this.CopyLocalSourcePdbToSymbolCache)
                    {
                        foreach (var packInput in this.PackInputs!)
                        {
                            if (Path.GetExtension(packInput.ItemSpec) == PdbFileExtension)
                            {
                                this.copyPdbToSymbolCacheCommand.AddAndCleanCache(packInput.ItemSpec, this.SymbolCacheDir, settings, this.commandLogger);
                            }
                        }
                    }
                }
                else
                {
                    this.pushPackageCommand.PushAsync(
                        packagePath,
                        source,
                        this.ApiKey,
                        symbolPackagePath,
                        this.SymbolsSource,
                        this.SymbolApiKey,
                        this.TimeoutInSeconds,
                        settings,
                        this.NoServiceEndpoint,
                        this.SkipDuplicate,
                        new NuGetToMsBuildLoggerAdapter(this.Log),
                        this.commandLogger).Wait();
                }
            }

            if (source != null && this.PublishLogFormats != null && (!isLocalSource || this.AllowLocalSource))
            {
                PublishLogger.Log(this.commandLogger, this.PublishLogFormats, this.PackageId!, this.Version!, source, packagePath);
            }

            var packagePathTaskItem = new TaskItem(packagePath);
            packagePathTaskItem.SetMetadata(PackageSourceText, this.Source);
            packagePathTaskItem.SetMetadata(PublishedText, this.PublishPackages.ToString(CultureInfo.InvariantCulture));
            packagePathTaskItem.SetMetadata(IsSymbolText, false.ToString(CultureInfo.InvariantCulture));
            this.PackagePaths = new ITaskItem[symbolPackagePath != null ? 2 : 1];
            this.PackagePaths[0] = packagePathTaskItem;
            if (symbolPackagePath != null)
            {
                var symbolsPackagePath = new TaskItem(symbolPackagePath);
                symbolsPackagePath.SetMetadata(PackageSourceText, this.SymbolsSource);
                symbolsPackagePath.SetMetadata(PublishedText, this.PublishPackages.ToString(CultureInfo.InvariantCulture));
                symbolsPackagePath.SetMetadata(IsSymbolText, true.ToString(CultureInfo.InvariantCulture));
                this.PackagePaths[1] = symbolsPackagePath;
            }

            return true;
        }

        private string GetPackagePathWithoutExtension()
        {
            return Path.Combine(this.ProjectDir!, this.PackageOutputPath!, $"{this.PackageId}.{this.Version}");
        }
    }
}
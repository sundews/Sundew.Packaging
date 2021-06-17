// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PushFacade.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Push
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using global::NuGet.Commands;
    using global::NuGet.Common;
    using global::NuGet.Configuration;
    using Sundew.Packaging.Tool.RegularExpression;
    using Sundew.Packaging.Versioning.IO;
    using Sundew.Packaging.Versioning.NuGet.Configuration;

    public class PushFacade
    {
        private const string AllFilesSearchPattern = "*";
        private const string SnupkgExtension = ".snupkg";
        private const string SymbolsNupkgExtension = ".symbols.nupkg";
        private readonly IFileSystem fileSystem;
        private readonly ISettingsFactory settingsFactory;
        private readonly ILogger nuGetLogger;

        public PushFacade(IFileSystem fileSystem, ISettingsFactory settingsFactory, ILogger nuGetLogger)
        {
            this.fileSystem = fileSystem;
            this.settingsFactory = settingsFactory;
            this.nuGetLogger = nuGetLogger;
        }

        public async Task PushAsync(PushVerb pushVerb)
        {
            var workingDirectory = pushVerb.WorkingDirectory ?? this.fileSystem.GetCurrentDirectory();
            var settings = this.settingsFactory.LoadDefaultSettings(workingDirectory);
            var packageSourceProvider = new PackageSourceProvider(settings);
            var files = pushVerb.PackagePaths.Select(
                pathPattern =>
            {
                var globRegex = GlobRegex.Create(pathPattern);
                var directory = Path.GetDirectoryName(Path.GetFullPath(pathPattern));
                if (directory == null)
                {
                    return Enumerable.Empty<string>();
                }

                return this.fileSystem.EnumerableFiles(directory, AllFilesSearchPattern, SearchOption.AllDirectories)
                    .Where(file => globRegex.IsMatch(file) && !file.EndsWith(SymbolsNupkgExtension));
            }).SelectMany(x => x).ToList();

            if (files.Count > 0)
            {
                await PushRunner.Run(
                    settings,
                    packageSourceProvider,
                    files,
                    pushVerb.PushSource,
                    pushVerb.ApiKey,
                    null,
                    null,
                    pushVerb.TimeoutSeconds,
                    false,
                    true,
                    true,
                    pushVerb.SkipDuplicate,
                    this.nuGetLogger);

                if (!pushVerb.NoSymbols)
                {
                    var symbols = files.Select(
                        x =>
                        {
                            var symbolsPath = Path.ChangeExtension(x, SnupkgExtension);
                            if (!this.fileSystem.FileExists(symbolsPath))
                            {
                                return Path.ChangeExtension(x, SymbolsNupkgExtension);
                            }

                            return symbolsPath;
                        }).Where(x => this.fileSystem.FileExists(x)).ToList();
                    await PushRunner.Run(
                    settings,
                    packageSourceProvider,
                    symbols,
                    pushVerb.SymbolsPushSource,
                    pushVerb.SymbolsApiKey,
                    null,
                    null,
                    pushVerb.TimeoutSeconds,
                    false,
                    pushVerb.NoSymbols,
                    true,
                    pushVerb.SkipDuplicate,
                    this.nuGetLogger);
                }
            }
            else
            {
                this.nuGetLogger.LogInformation("Found no packages to push.");
            }
        }
    }
}

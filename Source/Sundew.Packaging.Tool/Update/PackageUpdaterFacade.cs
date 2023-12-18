// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageUpdaterFacade.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Update;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Sundew.Base;
using Sundew.Packaging.RegularExpression;
using Sundew.Packaging.Tool.Diagnostics;
using Sundew.Packaging.Tool.Update.MsBuild;
using Sundew.Packaging.Tool.Update.MsBuild.NuGet;

/// <summary>
/// Facade for updating package references.
/// </summary>
public sealed class PackageUpdaterFacade
{
    private readonly IFileSystem fileSystem;
    private readonly PackageRestorer packageRestorer;
    private readonly IPackageUpdaterFacadeReporter packageUpdaterFacadeReporter;
    private readonly PackageVersionUpdater packageVersionUpdater;
    private readonly PackageVersionSelector packageVersionSelector;
    private readonly MsBuildProjectPackagesParser msBuildProjectPackagesParser;
    private readonly MsBuildProjectFileSearcher msBuildProjectFileSearcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackageUpdaterFacade"/> class.
    /// </summary>
    /// <param name="fileSystem">The file system.</param>
    /// <param name="nuGetPackageVersionFetcher">The nu get package version fetcher.</param>
    /// <param name="processRunner">The process runner.</param>
    /// <param name="packageUpdaterFacadeReporter">The package updater facade reporter.</param>
    /// <param name="packageVersionUpdaterReporter">The package version updater reporter.</param>
    /// <param name="packageVersionSelectorReporter">The package version selector reporter.</param>
    /// <param name="packageRestorerReporter">The package restorer reporter.</param>
    public PackageUpdaterFacade(
        IFileSystem fileSystem,
        INuGetPackageVersionFetcher nuGetPackageVersionFetcher,
        IProcessRunner processRunner,
        IPackageUpdaterFacadeReporter packageUpdaterFacadeReporter,
        IPackageVersionUpdaterReporter packageVersionUpdaterReporter,
        IPackageVersionSelectorReporter packageVersionSelectorReporter,
        IPackageRestorerReporter packageRestorerReporter)
    {
        this.fileSystem = fileSystem;
        this.packageRestorer = new PackageRestorer(processRunner, packageRestorerReporter);
        this.packageUpdaterFacadeReporter = packageUpdaterFacadeReporter;
        this.packageVersionUpdater = new PackageVersionUpdater(packageVersionUpdaterReporter);
        this.packageVersionSelector = new PackageVersionSelector(nuGetPackageVersionFetcher, packageVersionSelectorReporter);
        this.msBuildProjectPackagesParser = new MsBuildProjectPackagesParser(fileSystem.File);
        this.msBuildProjectFileSearcher = new MsBuildProjectFileSearcher(fileSystem.Directory);
    }

    /// <summary>
    /// Updates the packages in projects asynchronous.
    /// </summary>
    /// <param name="updateVerb">The update verb.</param>
    /// <returns>A task for the completion.</returns>
    public async Task UpdatePackagesInProjectsAsync(UpdateVerb updateVerb)
    {
        var rootDirectory = updateVerb.RootDirectory ?? this.fileSystem.Directory.GetCurrentDirectory();
        this.packageUpdaterFacadeReporter.StartingPackageUpdate(rootDirectory);
        var changedProjects = new List<MsBuildProject>();
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var globalGlobRegex = string.IsNullOrEmpty(updateVerb.VersionPattern)
                ? null
                : GlobRegex.Create(updateVerb.VersionPattern);
            foreach (var project in this.msBuildProjectFileSearcher.GetProjects(rootDirectory, updateVerb.Projects).ToList())
            {
                this.packageUpdaterFacadeReporter.UpdatingProject(project);

                var msBuildProject = await this.msBuildProjectPackagesParser.GetPackages(project, updateVerb.PackageIds);
                var packageUpdates = await this.packageVersionSelector.GetPackageVersions(msBuildProject.PossiblePackageUpdates, globalGlobRegex, rootDirectory, updateVerb.AllowPrerelease, updateVerb.Source);
                var result = this.packageVersionUpdater.TryUpdateAsync(msBuildProject, packageUpdates);
                if (result.HasValue())
                {
                    changedProjects.Add(result);
                }
            }

            foreach (var changedProject in changedProjects)
            {
                await this.fileSystem.File.WriteAllTextAsync(changedProject.Path, changedProject.ProjectContent);
            }

            if (changedProjects.Count > 0 && !updateVerb.SkipRestore)
            {
                await this.packageRestorer.RestoreAsync(rootDirectory, updateVerb.Verbose);
            }
        }
        catch (Exception e)
        {
            this.packageUpdaterFacadeReporter.Exception(e);
            return;
        }

        this.packageUpdaterFacadeReporter.CompletedPackageUpdate(changedProjects, updateVerb.SkipRestore, stopwatch.Elapsed);
    }
}
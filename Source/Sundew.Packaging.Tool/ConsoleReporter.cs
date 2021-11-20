// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool;

using System;
using System.Collections.Generic;
using global::NuGet.Versioning;
using Sundew.Base.Text;
using Sundew.Packaging.Tool.AwaitPublish;
using Sundew.Packaging.Tool.Delete;
using Sundew.Packaging.Tool.PruneLocalSource;
using Sundew.Packaging.Tool.Update;
using Sundew.Packaging.Tool.Update.MsBuild;
using Sundew.Packaging.Tool.Update.MsBuild.NuGet;
using Sundew.Packaging.Tool.Versioning;

internal class ConsoleReporter :
    IPackageVersionUpdaterReporter,
    IPackageUpdaterFacadeReporter,
    IPackageVersionSelectorReporter,
    IPackageRestorerReporter,
    IAwaitPublishFacadeReporter,
    IPruneReporter,
    IDeleteFacadeReporter,
    IStageBuildLogger
{
    private const string ModifiedVerbose = "Updated";
    private const string Modified = "*";
    private const string UnmodifiedVerbose = "No changes for";
    private const string Unmodified = " ";
    private readonly bool verbose;

    public ConsoleReporter(bool verbose)
    {
        this.verbose = verbose;
    }

    public void ProcessedProject(string projectPath, bool wasModified)
    {
        Console.WriteLine(this.verbose
            ? $" {(wasModified ? ModifiedVerbose : UnmodifiedVerbose)} {projectPath}"
            : $" {(wasModified ? Modified : Unmodified)} {projectPath}");
    }

    public void StartingPackageUpdate(string rootDirectory)
    {
        Console.WriteLine($"Updating packages in: {rootDirectory}");
    }

    public void UpdatingProject(string project)
    {
        if (this.verbose)
        {
            Console.WriteLine($" Updating packages for: {project}");
        }
    }

    public void CompletedPackageUpdate(List<MsBuildProject> msBuildProjects, bool skippedRestore, TimeSpan totalTime)
    {
        Console.WriteLine($"Updated{(skippedRestore ? string.Empty : " and restored")}: {msBuildProjects.Count} projects in: {totalTime}");
    }

    public void Exception(Exception exception)
    {
        var backgroundColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(exception.ToString());
        Console.ForegroundColor = backgroundColor;
    }

    public void PackageExistsResult(PackageIdAndVersion packageIdAndVersion, bool packageExists)
    {
        if (this.verbose)
        {
            Console.WriteLine($"Package: {packageIdAndVersion.Id}.{packageIdAndVersion.NuGetVersion} was {(packageExists ? string.Empty : "not ")}found");
        }
    }

    public void StartWaitingForPackage(PackageIdAndVersion packageIdAndVersion, string source)
    {
        Console.WriteLine($"Waiting for: {packageIdAndVersion.Id}.{packageIdAndVersion.NuGetVersion} in {source}");
    }

    public void CompletedWaitingForPackage(PackageIdAndVersion packageIdAndVersion, bool packageExists, TimeSpan stopwatchElapsed)
    {
        Console.WriteLine($"{(packageExists ? "Successfully waited" : "Timed out when waiting")} for: {packageIdAndVersion.Id}.{packageIdAndVersion.NuGetVersion}");
    }

    public void ReportMessage(string message)
    {
        Console.WriteLine(message);
    }

    public void PackageUpdateSelected(string packageId, NuGetVersion? oldNuGetVersion, NuGetVersion newNuGetVersion)
    {
        if (this.verbose)
        {
            Console.WriteLine($"  Updated {packageId} from {oldNuGetVersion} to {newNuGetVersion}");
        }
    }

    public void StartPruning(string source, IReadOnlyList<string> packageIds)
    {
        const string separator = ", ";
        Console.WriteLine($"Pruning matches of {packageIds.JoinToString(separator)} in: {source} ");
    }

    public void StartingDelete(string rootedFileSpecification)
    {
        if (this.verbose)
        {
            Console.WriteLine($"Deleting {rootedFileSpecification}");
        }
    }

    public void Deleted(string file)
    {
        if (this.verbose)
        {
            Console.WriteLine($"  Deleted {file}");
        }
    }

    public void CompletedDeleting(bool wasSuccessful, int numberFilesDeleted, TimeSpan stopwatchElapsed)
    {
        if (wasSuccessful)
        {
            Console.WriteLine($"Deleted {numberFilesDeleted} file(s)");
            return;
        }

        Console.WriteLine($"Canceled deleting... ({numberFilesDeleted} files deleted)");
    }

    public void CompletedPruning(bool wasSuccessful, int numberDirectoriesPurged, TimeSpan stopwatchElapsed)
    {
        if (wasSuccessful)
        {
            Console.WriteLine($"Pruned {numberDirectoriesPurged} directories");
            return;
        }

        Console.WriteLine($"Canceled pruning... ({numberDirectoriesPurged} directories deleted)");
    }
}
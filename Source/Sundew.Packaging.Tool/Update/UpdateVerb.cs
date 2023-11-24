// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateVerb.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Update;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Sundew.CommandLine;
using Sundew.Packaging.Source;
using Sundew.Packaging.Tool.Update.MsBuild.NuGet;

/// <summary>
/// Verb for updating package references.
/// </summary>
/// <seealso cref="Sundew.CommandLine.IVerb" />
public class UpdateVerb : IVerb
{
    private const string Star = "*";
    private const string VersionRegexText = @"(?<Version>[\d\.\*]+(?:(?:-(?<Prerelease>[^\.\s]+))|$))";
    private const string All = "All";
    private static readonly Regex PackageIdAndVersionRegex = new(@$"(?: |\.){VersionRegexText}");
    private static readonly Regex VersionRegex = new(VersionRegexText);
    private readonly List<PackageId> packageIds;
    private readonly List<string> projects;
    private bool useLocalSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateVerb"/> class.
    /// </summary>
    public UpdateVerb()
        : this(new List<PackageId> { new(Star) }, new List<string> { Star })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateVerb"/> class.
    /// </summary>
    /// <param name="packageIds">The package ids.</param>
    /// <param name="projects">The projects.</param>
    /// <param name="source">The source.</param>
    /// <param name="versionPattern">The version pattern.</param>
    /// <param name="rootDirectory">The root directory.</param>
    /// <param name="allowPrerelease">if set to <c>true</c> [allow prerelease].</param>
    /// <param name="verbose">if set to <c>true</c> [verbose].</param>
    /// <param name="useLocalSource">if set to <c>true</c> [use local source].</param>
    /// <param name="skipRestore">if set to <c>true</c> [skip restore].</param>
    public UpdateVerb(List<PackageId> packageIds, List<string> projects, string? source = null, string? versionPattern = null, string? rootDirectory = null, bool allowPrerelease = false, bool verbose = false, bool useLocalSource = false, bool skipRestore = false)
    {
        this.packageIds = packageIds;
        this.projects = projects;
        this.Source = source ?? All;
        this.VersionPattern = versionPattern;
        this.RootDirectory = rootDirectory;
        this.AllowPrerelease = allowPrerelease;
        this.Verbose = verbose;
        this.UseLocalSource = useLocalSource;
        this.SkipRestore = skipRestore;
    }

    /// <summary>
    /// Gets the next verb.
    /// </summary>
    /// <value>
    /// The next verb.
    /// </value>
    public IVerb? NextVerb { get; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; } = "update";

    /// <summary>
    /// Gets the short name.
    /// </summary>
    /// <value>
    /// The short name.
    /// </value>
    public string? ShortName { get; } = "u";

    /// <summary>
    /// Gets the help text.
    /// </summary>
    /// <value>
    /// The help text.
    /// </value>
    public string HelpText { get; } = "Update package references";

    /// <summary>
    /// Gets the package ids.
    /// </summary>
    /// <value>
    /// The package ids.
    /// </value>
    public IReadOnlyList<PackageId> PackageIds => this.packageIds;

    /// <summary>
    /// Gets the projects.
    /// </summary>
    /// <value>
    /// The projects.
    /// </value>
    public IReadOnlyList<string> Projects => this.projects;

    /// <summary>
    /// Gets the source.
    /// </summary>
    /// <value>
    /// The source.
    /// </value>
    public string? Source { get; private set; }

    /// <summary>
    /// Gets the version pattern.
    /// </summary>
    /// <value>
    /// The version pattern.
    /// </value>
    public string? VersionPattern { get; private set; }

    /// <summary>
    /// Gets the root directory.
    /// </summary>
    /// <value>
    /// The root directory.
    /// </value>
    public string? RootDirectory { get; private set; }

    /// <summary>
    /// Gets a value indicating whether [allow prerelease].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [allow prerelease]; otherwise, <c>false</c>.
    /// </value>
    public bool AllowPrerelease { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="UpdateVerb"/> is verbose.
    /// </summary>
    /// <value>
    ///   <c>true</c> if verbose; otherwise, <c>false</c>.
    /// </value>
    public bool Verbose { get; private set; }

    /// <summary>
    /// Gets a value indicating whether [use local source].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [use local source]; otherwise, <c>false</c>.
    /// </value>
    public bool UseLocalSource
    {
        get => this.useLocalSource;
        private set
        {
            this.useLocalSource = value;
            if (this.UseLocalSource)
            {
                this.Source = PackageSources.DefaultLocalSourceName;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether [skip restore].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [skip restore]; otherwise, <c>false</c>.
    /// </value>
    public bool SkipRestore { get; private set; }

    /// <summary>
    /// Configures the specified arguments builder.
    /// </summary>
    /// <param name="argumentsBuilder">The arguments builder.</param>
    public void Configure(IArgumentsBuilder argumentsBuilder)
    {
        argumentsBuilder.AddOptionalList("id", "package-ids", this.packageIds, this.SerializePackageId, this.DeserializePackageId, @$"The package(s) to update. (* Wildcards supported){Environment.NewLine}Format: Id[.Version] or ""Id[ Version]"" (Pinning version is optional)");
        argumentsBuilder.AddOptionalList("p", "projects", this.projects, "The project(s) to update (* Wildcards supported)");
        argumentsBuilder.AddOptional("s", "source", () => this.Source, s => this.Source = s, @"The source or source name to search for packages (""All"" supported)", defaultValueText: "NuGet.config: All");
        argumentsBuilder.AddOptional(null, "version", () => this.VersionPattern, s => this.VersionPattern = this.DeserializeVersion(s), "The NuGet package version (* Wildcards supported).", defaultValueText: "Latest version");
        CommonOptions.AddRootDirectory(argumentsBuilder, () => this.RootDirectory, s => this.RootDirectory = s);
        argumentsBuilder.AddSwitch("pr", "prerelease", this.AllowPrerelease, b => this.AllowPrerelease = b, "Allow updating to latest prerelease version");
        CommonOptions.AddVerbose(argumentsBuilder, this.Verbose, b => this.Verbose = b);
        argumentsBuilder.AddSwitch("l", "local", this.UseLocalSource, b => this.UseLocalSource = b, $@"Forces the source to ""{PackageSources.DefaultLocalSourceName}""");
        argumentsBuilder.AddSwitch("sr", "skip-restore", this.SkipRestore, b => this.SkipRestore = b, "Skips a dotnet restore command after package update.");
    }

    private string? DeserializeVersion(string pinnedNuGetVersion)
    {
        var match = VersionRegex.Match(pinnedNuGetVersion);
        if (match.Success)
        {
            return match.Value;
        }

        throw new ArgumentException($"Invalid version: {pinnedNuGetVersion}", nameof(pinnedNuGetVersion));
    }

    private string SerializePackageId(PackageId id, CultureInfo cultureInfo)
    {
        if (id.VersionPattern != null)
        {
            return $"{id.Id}.{id.VersionPattern}";
        }

        return id.Id;
    }

    private PackageId DeserializePackageId(string id, CultureInfo cultureInfo)
    {
        var match = PackageIdAndVersionRegex.Match(id);
        if (match.Success)
        {
            var versionGroup = match.Groups[CommonOptions.VersionGroupName];
            if (versionGroup.Success)
            {
                return new PackageId(id.Substring(0, versionGroup.Index - 1), versionGroup.Value);
            }
        }

        return new PackageId(id);
    }
}
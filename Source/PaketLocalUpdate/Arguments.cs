// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Arguments.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PaketLocalUpdate;

using Sundew.CommandLine;
using Sundew.Packaging.Source;

/// <summary>
/// Command line arguments for performing a local paket update.
/// </summary>
/// <seealso cref="Sundew.CommandLine.IArguments" />
public class Arguments : IArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Arguments"/> class.
    /// </summary>
    public Arguments()
        : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Arguments"/> class.
    /// </summary>
    /// <param name="packageId">The package identifier.</param>
    /// <param name="source">The source.</param>
    /// <param name="group">The group.</param>
    /// <param name="version">The version.</param>
    /// <param name="isFilter">if set to <c>true</c> [is filter].</param>
    /// <param name="isVerbose">if set to <c>true</c> [is verbose].</param>
    public Arguments(string? packageId = null, string? source = null, string? group = null, string? version = null, bool isFilter = false, bool isVerbose = false)
    {
        this.PackageId = packageId ?? "*";
        this.Source = source ?? PackageSources.DefaultLocalSourceName;
        this.Group = @group ?? "Main";
        this.Version = version;
        this.IsFilter = isFilter;
        this.IsVerbose = isVerbose;
    }

    /// <summary>
    /// Gets the source.
    /// </summary>
    /// <value>
    /// The source.
    /// </value>
    public string Source { get; private set; }

    /// <summary>
    /// Gets the group.
    /// </summary>
    /// <value>
    /// The group.
    /// </value>
    public string Group { get; private set; }

    /// <summary>
    /// Gets the package identifier.
    /// </summary>
    /// <value>
    /// The package identifier.
    /// </value>
    public string PackageId { get; private set; }

    /// <summary>
    /// Gets the version.
    /// </summary>
    /// <value>
    /// The version.
    /// </value>
    public string? Version { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance is filter.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is filter; otherwise, <c>false</c>.
    /// </value>
    public bool IsFilter { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance is verbose.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is verbose; otherwise, <c>false</c>.
    /// </value>
    public bool IsVerbose { get; private set; }

    /// <summary>
    /// Gets the help text.
    /// </summary>
    /// <value>
    /// The help text.
    /// </value>
    public string HelpText { get; } = "Update and restore local packages without paket.local files";

    /// <summary>
    /// Configures the specified arguments builder.
    /// </summary>
    /// <param name="argumentsBuilder">The arguments builder.</param>
    public void Configure(IArgumentsBuilder argumentsBuilder)
    {
        argumentsBuilder.AddOptional("s", "source", () => this.Source, s => this.Source = s, @"The local source or its name to be temporarily added to search for packages.");
        argumentsBuilder.AddOptional("g", "group", () => this.Group, s => this.Group = s, "The group name");
        argumentsBuilder.AddOptional("V", "version", () => this.Version, s => this.Version = s, "The version constraint");
        argumentsBuilder.AddSwitch("f", "filter", this.IsFilter, b => this.IsFilter = b, "Specifies whether the package id should be treated as a regex");
        argumentsBuilder.AddSwitch("v", "verbose", this.IsVerbose, b => this.IsVerbose = b, "Enable verbose logging");
        argumentsBuilder.AddOptionalValue("package-id", () => this.PackageId, s => this.PackageId = s, "Package id or pattern");
    }
}
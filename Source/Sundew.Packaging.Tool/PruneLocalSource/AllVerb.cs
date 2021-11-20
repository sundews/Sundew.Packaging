// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AllVerb.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.PruneLocalSource;

using System.Collections.Generic;
using Sundew.CommandLine;
using Sundew.Packaging.Source;

/// <summary>
/// Verb for refering to all package ids.
/// </summary>
/// <seealso cref="Sundew.CommandLine.IVerb" />
public class AllVerb : IVerb
{
    private readonly List<string> packageIds;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllVerb"/> class.
    /// </summary>
    public AllVerb()
        : this(new List<string> { "*" }, PackageSources.DefaultLocalSourceName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AllVerb"/> class.
    /// </summary>
    /// <param name="packageIds">The package ids.</param>
    /// <param name="source">The source.</param>
    /// <param name="verbose">if set to <c>true</c> [verbose].</param>
    public AllVerb(List<string> packageIds, string source, bool verbose = false)
    {
        this.packageIds = packageIds;
        this.Source = source;
        this.Verbose = verbose;
    }

    /// <summary>
    /// Gets the source.
    /// </summary>
    /// <value>
    /// The source.
    /// </value>
    public string Source { get; private set; }

    /// <summary>
    /// Gets the package ids.
    /// </summary>
    /// <value>
    /// The package ids.
    /// </value>
    public IReadOnlyList<string> PackageIds => this.packageIds;

    /// <summary>
    /// Gets a value indicating whether this <see cref="AllVerb"/> is verbose.
    /// </summary>
    /// <value>
    ///   <c>true</c> if verbose; otherwise, <c>false</c>.
    /// </value>
    public bool Verbose { get; private set; }

    /// <summary>
    /// Gets the next verb.
    /// </summary>
    /// <value>
    /// The next verb.
    /// </value>
    public IVerb? NextVerb { get; } = default;

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; } = "all";

    /// <summary>
    /// Gets the short name.
    /// </summary>
    /// <value>
    /// The short name.
    /// </value>
    public string? ShortName => default;

    /// <summary>
    /// Gets the help text.
    /// </summary>
    /// <value>
    /// The help text.
    /// </value>
    public string HelpText { get; } = "Prune the specified local source for all packages";

    /// <summary>
    /// Configures the specified arguments builder.
    /// </summary>
    /// <param name="argumentsBuilder">The arguments builder.</param>
    public void Configure(IArgumentsBuilder argumentsBuilder)
    {
        argumentsBuilder.AddOptionalList("p", "package-ids", this.packageIds, "The packages to prune (* Wildcards supported)");
        argumentsBuilder.AddOptional("s", "source", () => this.Source, s => this.Source = s, @"Local source or source name to search for packages");
        CommonOptions.AddVerbose(argumentsBuilder, this.Verbose, b => this.Verbose = b);
    }
}
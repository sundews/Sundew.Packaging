// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StageBuildVerb.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Versioning;

using System.Collections.Generic;
using System.IO;
using Sundew.CommandLine;
using Sundew.Packaging.Versioning;

/// <summary>
/// Verb for staging a buildtage.
/// </summary>
/// <seealso cref="Sundew.CommandLine.IVerb" />
public class StageBuildVerb : IVerb
{
    private readonly List<string> outputFormats;

    /// <summary>
    /// Initializes a new instance of the <see cref="StageBuildVerb"/> class.
    /// </summary>
    public StageBuildVerb()
    {
        this.ProjectFile = string.Empty;
        this.Stage = string.Empty;
        this.outputFormats = new List<string>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StageBuildVerb" /> class.
    /// </summary>
    /// <param name="packageInfo">The package information.</param>
    /// <param name="stage">The stage.</param>
    /// <param name="production">The production.</param>
    /// <param name="integration">The integration.</param>
    /// <param name="development">The development.</param>
    /// <param name="fallback">The fallback.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="workingDirectory">The working directory.</param>
    /// <param name="versioningMode">The versioning mode.</param>
    /// <param name="prereleasePrefix">The prerelease prefix.</param>
    /// <param name="prereleasePostfix">The prerelease postfix.</param>
    /// <param name="prereleaseFormat">The prerelease format.</param>
    /// <param name="metadata">The metadata.</param>
    /// <param name="versionFormat">The version format.</param>
    /// <param name="forceVersion">The force version.</param>
    /// <param name="outputFormats">The output formats.</param>
    public StageBuildVerb(
        string packageInfo,
        string stage,
        string? production = null,
        string? integration = null,
        string? development = null,
        string? fallback = null,
        string? configuration = null,
        string? workingDirectory = null,
        VersioningMode versioningMode = VersioningMode.AutomaticLatestPatch,
        string? prereleasePrefix = null,
        string? prereleasePostfix = null,
        string? prereleaseFormat = null,
        string? metadata = null,
        string? versionFormat = null,
        string? forceVersion = null,
        List<string>? outputFormats = null)
    {
        this.ProjectFile = packageInfo;
        this.Stage = stage;
        this.Production = production;
        this.Integration = integration;
        this.Development = development;
        this.NoStageProperties = fallback;
        this.Configuration = configuration;
        this.WorkingDirectory = workingDirectory;
        this.VersioningMode = versioningMode;
        this.PrereleasePrefix = prereleasePrefix;
        this.PrereleasePostfix = prereleasePostfix;
        this.PrereleaseFormat = prereleaseFormat;
        this.Metadata = metadata;
        this.VersionFormat = versionFormat;
        this.ForceVersion = forceVersion;
        this.outputFormats = outputFormats ?? new List<string>();
    }

    /// <summary>
    /// Gets the next verb.
    /// </summary>
    /// <value>
    /// The next verb.
    /// </value>
    public IVerb? NextVerb => null;

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; } = "stage-build";

    /// <summary>
    /// Gets the name of the short.
    /// </summary>
    /// <value>
    /// The name of the short.
    /// </value>
    public string? ShortName => "sb";

    /// <summary>
    /// Gets the help text.
    /// </summary>
    /// <value>
    /// The help text.
    /// </value>
    public string HelpText => "Stages a build";

    /// <summary>
    /// Gets the project file.
    /// </summary>
    /// <value>
    /// The project file.
    /// </value>
    public string ProjectFile { get; private set; }

    /// <summary>
    /// Gets the stage.
    /// </summary>
    /// <value>
    /// The stage.
    /// </value>
    public string Stage { get; private set; }

    /// <summary>
    /// Gets the production.
    /// </summary>
    /// <value>
    /// The production.
    /// </value>
    public string? Production { get; private set; }

    /// <summary>
    /// Gets the integration.
    /// </summary>
    /// <value>
    /// The integration.
    /// </value>
    public string? Integration { get; private set; }

    /// <summary>
    /// Gets the development.
    /// </summary>
    /// <value>
    /// The development.
    /// </value>
    public string? Development { get; private set; }

    /// <summary>
    /// Gets the fallback.
    /// </summary>
    /// <value>
    /// The fallback.
    /// </value>
    public string? NoStageProperties { get; private set; }

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    /// <value>
    /// The configuration.
    /// </value>
    public string? Configuration { get; private set; }

    /// <summary>
    /// Gets the working directory.
    /// </summary>
    /// <value>
    /// The working directory.
    /// </value>
    public string? WorkingDirectory { get; private set; }

    /// <summary>
    /// Gets the versioning mode.
    /// </summary>
    /// <value>
    /// The versioning mode.
    /// </value>
    public VersioningMode VersioningMode { get; private set; }

    /// <summary>
    /// Gets the prerelease prefix.
    /// </summary>
    /// <value>
    /// The prerelease prefix.
    /// </value>
    public string? PrereleasePrefix { get; private set; }

    /// <summary>
    /// Gets the prerelease postfix.
    /// </summary>
    /// <value>
    /// The prerelease postfix.
    /// </value>
    public string? PrereleasePostfix { get; private set; }

    /// <summary>
    /// Gets the prerelease format.
    /// </summary>
    /// <value>
    /// The prerelease format.
    /// </value>
    public string? PrereleaseFormat { get; private set; }

    /// <summary>
    /// Gets the metadata.
    /// </summary>
    /// <value>
    /// The metadata.
    /// </value>
    public string? Metadata { get; private set; }

    /// <summary>
    /// Gets the version format.
    /// </summary>
    /// <value>
    /// The version format.
    /// </value>
    public string? VersionFormat { get; private set; }

    /// <summary>
    /// Gets the force version.
    /// </summary>
    /// <value>
    /// The force version.
    /// </value>
    public string? ForceVersion { get; private set; }

    /// <summary>
    /// Gets the output formats.
    /// </summary>
    /// <value>
    /// The output formats.
    /// </value>
    public IReadOnlyList<string>? OutputFormats => this.outputFormats;

    /// <summary>
    /// Configures the specified arguments builder.
    /// </summary>
    /// <param name="argumentsBuilder">The arguments builder.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "It's a multiline string.")]
    public void Configure(IArgumentsBuilder argumentsBuilder)
    {
        argumentsBuilder.AddRequired(
            "pf",
            "project-file",
            () => this.ProjectFile,
            s =>
            {
                this.ProjectFile = s;
                if (this.WorkingDirectory == null)
                {
                    this.WorkingDirectory = Path.GetDirectoryName(this.ProjectFile);
                }
            },
            "The project file");
        argumentsBuilder.AddRequired("s", "stage", () => this.Stage, e => this.Stage = e, "The stage used to match against the production, integration and development sources");
        argumentsBuilder.RequireAnyOf(
            "Stage selection",
            builder => builder
                .Add(
                    "p",
                    "production",
                    () => this.Production,
                    s => this.Production = s,
                    @"The production stage used to determine the stable version.
Format: Stage Regex =>[ #StagingName][ &PrereleaseVersionFormat] [ApiKey@]SourceUri[ {LatestVersionUri} ]
[ | [SymbolApiKey@]SymbolSourceUri][|[|PropertyName=PropertyValue]*]",
                    true)
                .Add("i", "integration", () => this.Integration, s => this.Integration = s, @"The integration stage used to determine the prerelease version.", true)
                .Add("d", "development", () => this.Development, s => this.Development = s, @"The development stage  used to determine the prerelease version.", true)
                .Add(
                    "n",
                    "no-stage",
                    () => this.NoStageProperties,
                    s => this.NoStageProperties = s,
                    @"The fallback stage and properties if no stage is matched.
[#StagingName|][PropertyName=PropertyValue]*",
                    true));
        argumentsBuilder.AddOptional("wd", "directory", () => this.WorkingDirectory, s => this.WorkingDirectory = s, "The working directory or file used to determine the base version.", true);
        argumentsBuilder.AddOptional("c", "configuration", () => this.Configuration, s => this.Configuration = s, "The configuration used to evaluate the project file.", true);
        argumentsBuilder.AddOptional("pp", "prerelease-prefix", () => this.PrereleasePrefix, s => this.PrereleasePrefix = s, "The prerelease prefix.");
        argumentsBuilder.AddOptional("ps", "prerelease-postfix", () => this.PrereleasePostfix, s => this.PrereleasePostfix = s, "The prerelease postfix.");
        argumentsBuilder.AddOptional(null, "prerelease-format", () => this.PrereleaseFormat, s => this.PrereleaseFormat = s, "The prerelease format.");
        argumentsBuilder.AddOptional("m", "metadata", () => this.Metadata, s => this.Metadata = s, "The version metadata.");
        argumentsBuilder.AddOptionalEnum("vm", "versioning-mode", () => this.VersioningMode, s => this.VersioningMode = s, "The versioning mode");
        argumentsBuilder.AddOptional("vf", "version-format", () => this.VersionFormat, s => this.VersionFormat = s, "The version format");
        argumentsBuilder.AddOptional("fv", "force-version", () => this.ForceVersion, s => this.ForceVersion = s, "Forces the version to the specified value");
        argumentsBuilder.AddOptionalList("o", "output-formats", this.outputFormats, "A list of formats that will be logged to stdout.");
    }
}
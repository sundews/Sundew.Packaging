// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AwaitPublishVerb.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.AwaitPublish;

using System;
using System.Text.RegularExpressions;
using global::NuGet.Versioning;
using Sundew.CommandLine;
using Sundew.Packaging.Tool.Update.MsBuild.NuGet;

/// <summary>
/// Verb for await a package to be published.
/// </summary>
/// <seealso cref="Sundew.CommandLine.IVerb" />
public class AwaitPublishVerb : IVerb
{
    internal static readonly Regex PackageIdAndVersionRegex = new(@"(?: |\.)(?<Version>(?:\d+\.\d+(?<Patch>\.\d+)?).*)");

    /// <summary>
    /// Initializes a new instance of the <see cref="AwaitPublishVerb"/> class.
    /// </summary>
    public AwaitPublishVerb()
        : this(default!)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AwaitPublishVerb"/> class.
    /// </summary>
    /// <param name="packageIdAndVersion">The package identifier and version.</param>
    /// <param name="source">The source.</param>
    /// <param name="rootDirectory">The root directory.</param>
    /// <param name="timeoutInSeconds">The timeout in seconds.</param>
    /// <param name="verbose">if set to <c>true</c> [verbose].</param>
    public AwaitPublishVerb(PackageIdAndVersion packageIdAndVersion, string? source = null, string? rootDirectory = null, int timeoutInSeconds = 300, bool verbose = false)
    {
        this.PackageIdAndVersion = packageIdAndVersion;
        this.Source = source;
        this.RootDirectory = rootDirectory;
        this.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
        this.Verbose = verbose;
    }

    /// <summary>
    /// Gets the next verb.
    /// </summary>
    /// <value>
    /// The next verb.
    /// </value>
    public IVerb? NextVerb { get; } = null;

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; } = "await";

    /// <summary>
    /// Gets the short name.
    /// </summary>
    /// <value>
    /// The short name.
    /// </value>
    public string? ShortName { get; } = "a";

    /// <summary>
    /// Gets the help text.
    /// </summary>
    /// <value>
    /// The help text.
    /// </value>
    public string HelpText { get; } = "Awaits the specified package to be published";

    /// <summary>
    /// Gets the package identifier and version.
    /// </summary>
    /// <value>
    /// The package identifier and version.
    /// </value>
    public PackageIdAndVersion PackageIdAndVersion { get; private set; }

    /// <summary>
    /// Gets the source.
    /// </summary>
    /// <value>
    /// The source.
    /// </value>
    public string? Source { get; private set; }

    /// <summary>
    /// Gets the root directory.
    /// </summary>
    /// <value>
    /// The root directory.
    /// </value>
    public string? RootDirectory { get; private set; }

    /// <summary>
    /// Gets the timeout.
    /// </summary>
    /// <value>
    /// The timeout.
    /// </value>
    public TimeSpan Timeout { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="AwaitPublishVerb"/> is verbose.
    /// </summary>
    /// <value>
    ///   <c>true</c> if verbose; otherwise, <c>false</c>.
    /// </value>
    public bool Verbose { get; private set; }

    /// <summary>
    /// Configures the specified arguments builder.
    /// </summary>
    /// <param name="argumentsBuilder">The arguments builder.</param>
    public void Configure(IArgumentsBuilder argumentsBuilder)
    {
        argumentsBuilder.AddOptional("s", "source", () => this.Source, s => this.Source = s, @"The source or source name to await publish", defaultValueText: "NuGet.config: defaultPushSource");
        CommonOptions.AddRootDirectory(argumentsBuilder, () => this.RootDirectory, s => this.RootDirectory = s);
        argumentsBuilder.AddOptional(
            "t",
            "timeout",
            (ci) => this.Timeout.TotalSeconds.ToString(ci),
            (s, ci) => this.Timeout = TimeSpan.FromSeconds(double.Parse(s, ci)),
            @"The wait timeout in seconds");
        argumentsBuilder.AddRequiredValue("package-id", this.SerializePackageId, this.DeserializePackageId, $"Specifies the package id and optionally the version{Environment.NewLine}Format: <PackageId>[.<Version>].{Environment.NewLine}If the version is not provided, it must be specified by the version value");
        argumentsBuilder.AddOptionalValue("version", this.SerializeVersion, this.DeserializeVersion, "Specifies the NuGet Package version");
        CommonOptions.AddVerbose(argumentsBuilder, this.Verbose, b => this.Verbose = b);
    }

    private string SerializeVersion()
    {
        if (this.PackageIdAndVersion == null)
        {
            return string.Empty;
        }

        return this.PackageIdAndVersion.NuGetVersion.ToFullString();
    }

    private string SerializePackageId()
    {
        if (this.PackageIdAndVersion == null)
        {
            return string.Empty;
        }

        if (this.PackageIdAndVersion.NuGetVersion == null)
        {
            return $"{this.PackageIdAndVersion.Id}";
        }

        return $"{this.PackageIdAndVersion.Id}.{this.PackageIdAndVersion.NuGetVersion}";
    }

    private void DeserializeVersion(string version)
    {
        this.PackageIdAndVersion = this.PackageIdAndVersion with { NuGetVersion = NuGetVersion.Parse(version) };
    }

    private void DeserializePackageId(string id)
    {
        var match = PackageIdAndVersionRegex.Match(id);
        if (match.Success)
        {
            var versionGroup = match.Groups[CommonOptions.VersionGroupName];
            if (versionGroup.Success)
            {
                this.PackageIdAndVersion = new PackageIdAndVersion(id.Substring(0, versionGroup.Index - 1), NuGetVersion.Parse(versionGroup.Value));
                return;
            }
        }

        this.PackageIdAndVersion = new PackageIdAndVersion(id, NuGetVersion.Parse("0.0.0"));
    }
}
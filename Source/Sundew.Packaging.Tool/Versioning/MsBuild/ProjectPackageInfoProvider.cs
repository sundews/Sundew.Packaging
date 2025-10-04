// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectPackageInfoProvider.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Versioning.MsBuild;

using System.IO;
using System.Linq;
using Microsoft.Build.Locator;
using Sundew.Packaging.Versioning.Logging;

/// <summary>
/// Providers info about a package for a project.
/// </summary>
/// <seealso cref="Sundew.Packaging.Tool.Versioning.MsBuild.IProjectPackageInfoProvider" />
public class ProjectPackageInfoProvider : IProjectPackageInfoProvider
{
    private const string Configuration = "Configuration";
    private const string Debug = "Debug";
    private const string PackageId = "PackageId";
    private const string PackageVersion = "PackageVersion";
    private const string Version100 = "1.0.0";
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectPackageInfoProvider"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ProjectPackageInfoProvider(ILogger logger)
    {
        this.logger = logger;
        if (!MSBuildLocator.IsRegistered)
        {
            var visualStudioInstance = MSBuildLocator.RegisterDefaults();
            this.logger.LogInfo($"Registered Name: {visualStudioInstance.Name}, Version: {visualStudioInstance.Version}, MsBuild: {visualStudioInstance.MSBuildPath}");
        }
    }

    /// <summary>
    /// Gets the package information.
    /// </summary>
    /// <param name="projectPath">The project path.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>
    /// The package info.
    /// </returns>
    public ProjectPackageInfo GetPackageInfo(string projectPath, string? configuration)
    {
        var properties = new System.Collections.Generic.Dictionary<string, string> { { Configuration, configuration ?? Debug }, };
        var projectCollection = new Microsoft.Build.Evaluation.ProjectCollection(properties);
        var project = projectCollection.LoadProject(projectPath);
        var packageId = project.Properties.Where(p => p.Name == PackageId).FirstOrDefault()?.EvaluatedValue ?? Path.GetFileNameWithoutExtension(projectPath);
        var packageVersion = project.Properties.Where(p => p.Name == PackageVersion).FirstOrDefault()?.EvaluatedValue ?? Version100;
        return new ProjectPackageInfo(packageId, packageVersion);
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IProjectPackageInfoProvider.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Versioning.MsBuild
{
    /// <summary>
    /// Provides info about a package.
    /// </summary>
    public interface IProjectPackageInfoProvider
    {
        /// <summary>
        /// Gets the package information.
        /// </summary>
        /// <param name="projectPath">The project path.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The package info.</returns>
        ProjectPackageInfo GetPackageInfo(string projectPath, string? configuration);
    }
}
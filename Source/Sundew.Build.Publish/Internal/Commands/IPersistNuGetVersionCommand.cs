// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPersistNuGetVersionCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Internal.Commands
{
    /// <summary>
    /// Interface for implementing the persist NuGet version command.
    /// </summary>
    public interface IPersistNuGetVersionCommand
    {
        /// <summary>
        /// Saves the specified version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="outputPath">The output path.</param>
        /// <param name="outputName">Name of the output.</param>
        void Save(string version, string outputPath, string outputName);
    }
}
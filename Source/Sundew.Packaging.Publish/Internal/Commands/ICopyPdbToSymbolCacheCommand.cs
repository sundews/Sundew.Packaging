// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICopyPdbToSymbolCacheCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands
{
    using System.Collections.Generic;
    using global::NuGet.Configuration;

    /// <summary>Interface for implementing a command that copies pdb files to the symbol cache.</summary>
    public interface ICopyPdbToSymbolCacheCommand
    {
        /// <summary>
        /// Adds the specified PDB file path.
        /// </summary>
        /// <param name="pdbFilePaths">The PDB file paths.</param>
        /// <param name="symbolCacheDirectoryPath">The symbol cache directory path.</param>
        /// <param name="settings">The settings.</param>
        void AddAndCleanCache(IReadOnlyList<string> pdbFilePaths, string? symbolCacheDirectoryPath, ISettings settings);
    }
}
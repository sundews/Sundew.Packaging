// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICopyPdbToSymbolCacheCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Internal.Commands
{
    using global::NuGet.Configuration;

    /// <summary>Interface for implementing a command that copies pdb files to the symbol cache.</summary>
    public interface ICopyPdbToSymbolCacheCommand
    {
        /// <summary>Adds the specified PDB file path.</summary>
        /// <param name="pdbFilePath">The PDB file path.</param>
        /// <param name="symbolCacheDirectoryPath">The symbol cache directory path.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="commandLogger">The command logger.</param>
        void AddAndCleanCache(string pdbFilePath, string symbolCacheDirectoryPath, ISettings settings, ICommandLogger commandLogger);
    }
}
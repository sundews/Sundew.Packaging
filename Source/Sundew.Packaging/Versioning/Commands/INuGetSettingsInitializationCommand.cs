// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INuGetSettingsInitializationCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.Commands;

/// <summary>Interface for implementing a command that can add local sources.</summary>
public interface INuGetSettingsInitializationCommand
{
    /// <summary>Adds the specified local source to a NuGet.Config in solution dir.</summary>
    /// <param name="workingDirectory">The solution dir.</param>
    /// <param name="localSourceName">The local source name.</param>
    /// <param name="localSource">The default local source.</param>
    /// <returns>The actual local source.</returns>
    NuGetSettings Initialize(string workingDirectory, string localSourceName, string localSource);
}
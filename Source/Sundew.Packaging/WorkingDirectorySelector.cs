// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkingDirectorySelector.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging;

using System;
using System.IO;
using Sundew.Packaging.Versioning.IO;

/// <summary>
/// Selects the working directory.
/// </summary>
public static class WorkingDirectorySelector
{
    private const string UndefinedText = "*Undefined*";

    /// <summary>
    /// Gets the working directory.
    /// </summary>
    /// <param name="proposedWorkingDirectory">The proposed working directory.</param>
    /// <param name="fileSystem">The file system.</param>
    /// <returns>The working directory.</returns>
    /// <exception cref="ArgumentException">The working directory cannot be null. - workingDirectory.</exception>
    public static string GetWorkingDirectory(string? proposedWorkingDirectory, IFileSystem fileSystem)
    {
        var workingDirectory = proposedWorkingDirectory;
        if (workingDirectory == UndefinedText)
        {
            workingDirectory = Path.GetDirectoryName(fileSystem.GetCurrentDirectory());
        }

        if (workingDirectory == null)
        {
            throw new ArgumentException("The working directory cannot be null.", nameof(workingDirectory));
        }

        return workingDirectory;
    }
}
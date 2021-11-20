// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICopyPackageToLocalSourceCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands;

/// <summary>Interface for implementing a command that can add NuGet packages to an offline feed.</summary>
public interface ICopyPackageToLocalSourceCommand
{
    /// <summary>
    /// Adds the asynchronous.
    /// </summary>
    /// <param name="packageId">The package id.</param>
    /// <param name="packagePath">The package path.</param>
    /// <param name="source">The source.</param>
    /// <param name="skipDuplicate">Skips duplicate packages.</param>
    /// <returns>
    /// The destination path.
    /// </returns>
    string Add(string packageId, string packagePath, string source, bool skipDuplicate);
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileSystemAsync.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PaketLocalUpdate;

using System.Threading.Tasks;
using Sundew.Packaging.Versioning.IO;

/// <summary>
/// Extends <see cref="IFileSystem"/> with async methods.
/// </summary>
/// <seealso cref="Sundew.Packaging.Versioning.IO.IFileSystem" />
public interface IFileSystemAsync : IFileSystem
{
    /// <summary>
    /// Writes all text asynchronous.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="content">The content.</param>
    /// <returns>An async task.</returns>
    Task WriteAllTextAsync(string filePath, string content);

    /// <summary>
    /// Reads all text asynchronous.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>An async task with the text.</returns>
    Task<string> ReadAllTextAsync(string filePath);
}
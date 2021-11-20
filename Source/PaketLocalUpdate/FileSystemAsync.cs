// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileSystemAsync.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PaketLocalUpdate;

using System.IO;
using System.Threading.Tasks;
using Sundew.Packaging.Versioning.IO;

/// <summary>
/// Extends <see cref="FileSystem"/> with async methods.
/// </summary>
/// <seealso cref="Sundew.Packaging.Versioning.IO.FileSystem" />
/// <seealso cref="PaketLocalUpdate.IFileSystemAsync" />
public class FileSystemAsync : FileSystem, IFileSystemAsync
{
    /// <summary>
    /// Reads all text asynchronous.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>
    /// An async task with the text.
    /// </returns>
    public Task<string> ReadAllTextAsync(string filePath)
    {
        return File.ReadAllTextAsync(filePath);
    }

    /// <summary>
    /// Writes all text asynchronous.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="content">The content.</param>
    /// <returns>
    /// An async task.
    /// </returns>
    public Task WriteAllTextAsync(string filePath, string content)
    {
        return File.WriteAllTextAsync(filePath, content);
    }
}
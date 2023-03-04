// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileSystem.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.IO;

using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// File system implementation.
/// </summary>
/// <seealso cref="Sundew.Packaging.Versioning.IO.IFileSystem" />
public class FileSystem : IFileSystem
{
    /// <summary>
    /// Files the exists.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///   <c>true</c>, if the file path exists, otherwise <c>false</c>.
    /// </returns>
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    /// <summary>
    /// Deletes the file.
    /// </summary>
    /// <param name="path">The path.</param>
    public void DeleteFile(string path)
    {
        File.Delete(path);
    }

    /// <summary>
    /// Writes all text.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="contents">The contents.</param>
    public void WriteAllText(string path, string contents)
    {
        File.WriteAllText(path, contents);
    }

    /// <summary>
    /// Appends all text.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="contents">The contents.</param>
    public void AppendAllText(string path, string contents)
    {
        File.AppendAllText(path, contents);
    }

    /// <summary>
    /// Writes all text.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="contents">The contents.</param>
    /// <param name="encoding">The encoding.</param>
    public void WriteAllText(string path, string contents, Encoding encoding)
    {
        File.WriteAllText(path, contents, encoding);
    }

    /// <summary>
    /// Appends all text.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="contents">The contents.</param>
    /// <param name="encoding">The encoding.</param>
    public void AppendAllText(string path, string contents, Encoding encoding)
    {
        File.AppendAllText(path, contents, encoding);
    }

    /// <summary>
    /// Reads all text.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    /// The text.
    /// </returns>
    public string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    /// <summary>
    /// Directories the exists.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///   <c>true</c>, if the directory path exists, otherwise <c>false</c>.
    /// </returns>
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    /// <summary>
    /// Creates the directory.
    /// </summary>
    /// <param name="path">The path.</param>
    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    /// <summary>
    /// Deletes the directory.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="recursive">if set to <c>true</c> [recursive].</param>
    public void DeleteDirectory(string path, bool recursive)
    {
        Directory.Delete(path, recursive);
    }

    /// <summary>
    /// Copies the specified source path.
    /// </summary>
    /// <param name="sourcePath">The source path.</param>
    /// <param name="destinationPath">The destination path.</param>
    /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
    public void Copy(string sourcePath, string destinationPath, bool overwrite)
    {
        File.Copy(sourcePath, destinationPath, overwrite);
    }

    /// <summary>
    /// Enumerables the files.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="searchPattern">The search pattern.</param>
    /// <param name="searchOption">The search option.</param>
    /// <returns>
    /// The file paths.
    /// </returns>
    public IEnumerable<string> EnumerableFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.EnumerateFiles(path, searchPattern, searchOption);
    }

    /// <summary>
    /// Reads all bytes.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    /// The bytes.
    /// </returns>
    public byte[] ReadAllBytes(string path)
    {
        return File.ReadAllBytes(path);
    }

    /// <summary>
    /// Gets the current directory.
    /// </summary>
    /// <returns>
    /// The current directory.
    /// </returns>
    public string GetCurrentDirectory()
    {
        return Directory.GetCurrentDirectory();
    }
}
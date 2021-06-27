// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileSystem.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.IO
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// File system abstraction.
    /// </summary>
    public interface IFileSystem
    {
        /// <summary>
        /// Files the exists.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c>, if the file path exists, otherwise <c>false</c>.</returns>
        bool FileExists(string path);

        /// <summary>
        /// Directories the exists.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c>, if the directory path exists, otherwise <c>false</c>.</returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Creates the directory.
        /// </summary>
        /// <param name="path">The path.</param>
        void CreateDirectory(string path);

        /// <summary>
        /// Copies the specified source path.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        void Copy(string sourcePath, string destinationPath, bool overwrite);

        /// <summary>
        /// Enumerables the files.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="searchOption">The search option.</param>
        /// <returns>The file paths.</returns>
        IEnumerable<string> EnumerableFiles(string path, string searchPattern, SearchOption searchOption);

        /// <summary>
        /// Deletes the directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        void DeleteDirectory(string path, bool recursive);

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="path">The path.</param>
        void DeleteFile(string path);

        /// <summary>
        /// Writes all text.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="contents">The contents.</param>
        void WriteAllText(string path, string contents);

        /// <summary>
        /// Appends all text.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="contents">The contents.</param>
        void AppendAllText(string path, string contents);

        /// <summary>
        /// Reads all text.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The text.</returns>
        string ReadAllText(string path);

        /// <summary>
        /// Reads all bytes.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The bytes.</returns>
        byte[] ReadAllBytes(string path);

        /// <summary>
        /// Gets the current directory.
        /// </summary>
        /// <returns>The current directory.</returns>
        string GetCurrentDirectory();
    }
}
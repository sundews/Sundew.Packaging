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

    public interface IFileSystem
    {
        bool FileExists(string path);

        bool DirectoryExists(string path);

        void CreateDirectory(string path);

        void Copy(string sourcePath, string destinationPath, bool overwrite);

        IEnumerable<string> EnumerableFiles(string path, string searchPattern, SearchOption searchOption);

        void DeleteDirectory(string path, bool recursive);

        void DeleteFile(string path);

        void WriteAllText(string path, string contents);

        void AppendAllText(string path, string contents);

        string ReadAllText(string path);

        byte[] ReadAllBytes(string path);

        string GetCurrentDirectory();
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileSystem.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.IO
{
    using System.Collections.Generic;
    using System.IO;

    public class FileSystem : IFileSystem
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        public void AppendAllText(string path, string contents)
        {
            File.AppendAllText(path, contents);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public void DeleteDirectory(string path, bool recursive)
        {
            Directory.Delete(path, recursive);
        }

        public void Copy(string sourcePath, string destinationPath, bool overwrite)
        {
            File.Copy(sourcePath, destinationPath, overwrite);
        }

        public IEnumerable<string> EnumerableFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.EnumerateFiles(path, searchPattern, searchOption);
        }

        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}
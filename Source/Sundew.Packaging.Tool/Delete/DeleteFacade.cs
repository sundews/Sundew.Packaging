// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteFacade.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Delete
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Sundew.Packaging.Tool.RegularExpression;

    public class DeleteFacade
    {
        private const string AllFilesSearchPattern = "*.*";
        private const string Directory = nameof(Directory);
        private static readonly Regex DirectoryRegex = new(@"^[^*]*");
        private readonly IFileSystem fileSystem;
        private readonly IDeleteFacadeReporter deleteFacadeReporter;

        public DeleteFacade(IFileSystem fileSystem, IDeleteFacadeReporter deleteFacadeReporter)
        {
            this.fileSystem = fileSystem;
            this.deleteFacadeReporter = deleteFacadeReporter;
        }

        public Task<int> Delete(DeleteVerb deleteVerb)
        {
            var stopwatch = Stopwatch.StartNew();
            var numberFilesDeleted = 0;
            try
            {
                var rootDirectory = deleteVerb.RootDirectory ?? this.fileSystem.Directory.GetCurrentDirectory();
                foreach (var fileSpecification in deleteVerb.Files)
                {
                    var rootedFileSpecification = Path.IsPathRooted(fileSpecification) ? fileSpecification : Path.Combine(rootDirectory, fileSpecification);
                    this.deleteFacadeReporter.StartingDelete(rootedFileSpecification);
                    var globRegex = GlobRegex.Create(rootedFileSpecification);
                    var match = DirectoryRegex.Match(Path.GetDirectoryName(globRegex.Glob) ?? string.Empty);
                    var directory = Path.TrimEndingDirectorySeparator(match.Value);
                    if (this.fileSystem.Directory.Exists(directory))
                    {
                        var files = this.fileSystem.Directory
                            .EnumerateFiles(
                                directory,
                                AllFilesSearchPattern,
                                deleteVerb.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                        foreach (var file in files.Where(x => globRegex.IsMatch(x)))
                        {
                            this.fileSystem.File.Delete(file);
                            this.deleteFacadeReporter.Deleted(file);
                            numberFilesDeleted++;
                        }
                    }
                }

                this.deleteFacadeReporter.CompletedDeleting(true, numberFilesDeleted, stopwatch.Elapsed);
            }
            catch (OperationCanceledException)
            {
                this.deleteFacadeReporter.CompletedDeleting(false, numberFilesDeleted, stopwatch.Elapsed);
                return Task.FromResult(-3);
            }
            catch (Exception e)
            {
                this.deleteFacadeReporter.Exception(e);
                return Task.FromResult(-1);
            }

            return Task.FromResult(0);
        }
    }
}
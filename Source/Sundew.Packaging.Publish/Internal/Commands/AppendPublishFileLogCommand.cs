// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppendPublishFileLogCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands
{
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Sundew.Packaging.Publish.Internal.IO;

    internal class AppendPublishFileLogCommand : IAppendPublishFileLogCommand
    {
        private const string FormatGroupName = "Format";
        private const string FileNameGroupName = "FileName";
        private static readonly Regex AppendFilesRegex = new(@"(?:(?<Format>[^>]+)\s>\s?(?<FileName>[^|]+)(\s?\|\s?)?)+");
        private readonly IFileSystem fileSystem;

        public AppendPublishFileLogCommand(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// Appends the specified output directory.
        /// </summary>
        /// <param name="workingDirectory">The output directory.</param>
        /// <param name="packagePushFileAppendFormats">The package push file append formats.</param>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="version">The version.</param>
        /// <param name="packagePath">The package path.</param>
        /// <param name="stage">The stage.</param>
        /// <param name="source">The source.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="feedSource">The feed source.</param>
        /// <param name="symbolPackagePath">The symbol package path.</param>
        /// <param name="symbolsSource">The symbols source.</param>
        /// <param name="symbolApiKey">The symbol API key.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="commandLogger">The command logger.</param>
        public void Append(
            string workingDirectory,
            string packagePushFileAppendFormats,
            string packageId,
            string version,
            string packagePath,
            string stage,
            string source,
            string? apiKey,
            string feedSource,
            string? symbolPackagePath,
            string? symbolsSource,
            string? symbolApiKey,
            string parameter,
            ICommandLogger commandLogger)
        {
            var match = AppendFilesRegex.Match(packagePushFileAppendFormats);
            if (match.Success)
            {
                var formatsAndFile = match.Groups[FormatGroupName].Captures.OfType<Capture>()
                    .Zip(match.Groups[FileNameGroupName].Captures.OfType<Capture>(), (formatCapture, fileCapture) => (format: formatCapture.Value, file: fileCapture.Value));
                foreach ((string format, string file) in formatsAndFile)
                {
                    var filePath = Path.IsPathRooted(file) ? file : Path.Combine(workingDirectory, file);
                    var directory = Path.GetDirectoryName(filePath);
                    if (!this.fileSystem.DirectoryExists(directory))
                    {
                        this.fileSystem.CreateDirectory(directory);
                    }

                    var contents = PublishLogger.Format(format, packageId, version, packagePath, stage, source, apiKey, feedSource, symbolPackagePath, symbolsSource, symbolApiKey, parameter);
                    this.fileSystem.AppendAllText(filePath, contents);
                    commandLogger.LogInfo($"Appended {contents} to {filePath}");
                }
            }
        }
    }
}
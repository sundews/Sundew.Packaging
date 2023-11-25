// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppendPublishFileLogCommand.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands;

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sundew.Packaging.Versioning;
using Sundew.Packaging.Versioning.IO;
using Sundew.Packaging.Versioning.Logging;

internal class AppendPublishFileLogCommand : IAppendPublishFileLogCommand
{
    private const string FormatGroupName = "Format";
    private const string FileNameGroupName = "FileName";
    private static readonly Regex AppendFilesRegex = new(@"(?:(?<Format>[^>]+)\s>\s?(?<FileName>[^|]+)(\s?\|\s?)?)+");
    private readonly IFileSystem fileSystem;
    private readonly ILogger logger;

    public AppendPublishFileLogCommand(IFileSystem fileSystem, ILogger logger)
    {
        this.fileSystem = fileSystem;
        this.logger = logger;
    }

    /// <summary>
    /// Appends the specified output directory.
    /// </summary>
    /// <param name="workingDirectory">The output directory.</param>
    /// <param name="packagePushFileAppendFormats">The package push file append formats.</param>
    /// <param name="packageId">The package identifier.</param>
    /// <param name="packagePath">The package path.</param>
    /// <param name="symbolPackagePath">The symbol package path.</param>
    /// <param name="publishInfo">The publish information.</param>
    /// <param name="parameter">The parameter.</param>
    public void Append(
        string workingDirectory,
        string packagePushFileAppendFormats,
        string packageId,
        string packagePath,
        string? symbolPackagePath,
        PublishInfo publishInfo,
        string parameter)
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

                var (log, isValid) = PackagePublicationLogger.Format(format, packageId,  packagePath, symbolPackagePath, publishInfo, workingDirectory, parameter);
                if (isValid)
                {
                    this.fileSystem.AppendAllText(filePath, log);
                    this.logger.LogInfo($"Appended {log} to {filePath}");
                }
                else
                {
                    this.logger.LogInfo($"Not logging to {filePath} as the format included null values for indices: {log}");
                }
            }
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetBuildDateTimeTask.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish
{
    using System;
    using System.IO;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Sundew.Base.Primitives.Time;
    using Sundew.Packaging.Publish.Internal;
    using Sundew.Packaging.Publish.Internal.Logging;
    using Sundew.Packaging.Versioning.IO;
    using ILogger = Sundew.Packaging.Versioning.Logging.ILogger;

    /// <summary>
    /// MSBuild task for determining the build time.
    /// </summary>
    /// <seealso cref="Microsoft.Build.Utilities.Task" />
    public class GetBuildDateTimeTask : Task
    {
        private readonly ILogger logger;
        private readonly IFileSystem fileSystem;
        private readonly PrereleaseDateTimeProvider prereleaseDateTimeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetBuildDateTimeTask"/> class.
        /// </summary>
        public GetBuildDateTimeTask()
        : this(null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetBuildDateTimeTask" /> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="logger">The logger.</param>
        internal GetBuildDateTimeTask(IFileSystem? fileSystem, IDateTime? dateTime, ILogger? logger)
        {
            this.logger = logger ?? new MsBuildLogger(this.Log);
            this.fileSystem = fileSystem ?? new FileSystem();
            this.prereleaseDateTimeProvider = new PrereleaseDateTimeProvider(this.fileSystem, dateTime ?? new DateTimeProvider(), this.logger);
        }

        /// <summary>
        /// Gets or sets the solution dir.
        /// </summary>
        /// <value>
        /// The solution dir.
        /// </value>
        [Required]
        public string? SolutionDir { get; set; }

        /// <summary>
        /// Gets or sets the build info file path.
        /// </summary>
        /// <value>
        /// The build info file path.
        /// </value>
        [Required]
        public string? BuildInfoFilePath { get; set; }

        /// <summary>
        /// Gets or sets the package versions path.
        /// </summary>
        /// <value>
        /// The package versions path.
        /// </value>
        [Required]
        public string? PackageVersionsPath { get; set; }

        /// <summary>
        /// Must be implemented by derived class.
        /// </summary>
        /// <returns>
        /// true, if successful.
        /// </returns>
        public override bool Execute()
        {
            try
            {
                var workingDirectory = WorkingDirectorySelector.GetWorkingDirectory(this.SolutionDir, this.fileSystem);
                var buildInfoFilePath = Path.Combine(workingDirectory, this.BuildInfoFilePath ?? throw new ArgumentNullException(nameof(this.BuildInfoFilePath), $"{nameof(this.BuildInfoFilePath)} was not set."));
                var packageVersionsPath = Path.Combine(workingDirectory, this.PackageVersionsPath ?? throw new ArgumentNullException(nameof(this.PackageVersionsPath), $"{nameof(this.PackageVersionsPath)} was not set."));
                var buildInfoDirectory = Path.GetDirectoryName(buildInfoFilePath);
                if (!this.fileSystem.DirectoryExists(buildInfoDirectory))
                {
                    this.fileSystem.CreateDirectory(buildInfoDirectory);
                }

                if (this.fileSystem.DirectoryExists(packageVersionsPath))
                {
                    this.fileSystem.DeleteDirectory(packageVersionsPath, true);
                    this.fileSystem.CreateDirectory(packageVersionsPath);
                }

                if (!this.fileSystem.DirectoryExists(packageVersionsPath))
                {
                    this.fileSystem.CreateDirectory(packageVersionsPath);
                }

                this.prereleaseDateTimeProvider.SaveBuildDateTime(buildInfoFilePath);
            }
            catch (Exception e)
            {
                this.logger.LogError(e.ToString());
                return false;
            }

            return true;
        }
    }
}
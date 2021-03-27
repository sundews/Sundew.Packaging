// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetBuildDateTimeTask.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish
{
    using System;
    using System.Globalization;
    using System.IO;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Sundew.Base.Time;
    using Sundew.Packaging.Publish.Internal.IO;
    using Sundew.Packaging.Publish.Internal.Logging;
    using ILogger = Sundew.Packaging.Publish.Internal.Logging.ILogger;

    /// <summary>
    /// MSBuild task for determining the build time.
    /// </summary>
    /// <seealso cref="Microsoft.Build.Utilities.Task" />
    public class GetBuildDateTimeTask : Task
    {
        private readonly IDateTime dateTime;
        private readonly ILogger logger;
        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetBuildDateTimeTask"/> class.
        /// </summary>
        public GetBuildDateTimeTask()
        : this(null, new DateTimeProvider(), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetBuildDateTimeTask" /> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="logger">The logger.</param>
        internal GetBuildDateTimeTask(IFileSystem? fileSystem, IDateTime dateTime, ILogger? logger)
        {
            this.dateTime = dateTime;
            this.logger = logger ?? new MsBuildLogger(this.Log);
            this.fileSystem = fileSystem ?? new FileSystem();
        }

        /// <summary>
        /// Gets or sets the build date time file path.
        /// </summary>
        /// <value>
        /// The build date time file path.
        /// </value>
        [Required]
        public string? BuildDateTimeFilePath { get; set; }

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
                var buildDateTimeFilePath = this.BuildDateTimeFilePath ?? throw new ArgumentNullException(nameof(this.BuildDateTimeFilePath), $"{nameof(this.BuildDateTimeFilePath)} was not set.");
                var packageVersionsPath = this.PackageVersionsPath ?? throw new ArgumentNullException(nameof(this.PackageVersionsPath), $"{nameof(this.PackageVersionsPath)} was not set.");
                var directory = Path.GetDirectoryName(buildDateTimeFilePath);
                if (!this.fileSystem.DirectoryExists(directory))
                {
                    this.fileSystem.CreateDirectory(directory);
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

                var dateTime = this.dateTime.UtcTime.ToString(CultureInfo.InvariantCulture);
                this.logger.LogImportant($"Wrote build time: {dateTime} to {buildDateTimeFilePath}");
                this.fileSystem.WriteAllText(buildDateTimeFilePath, dateTime);
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
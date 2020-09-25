// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AdjustProjectReferenceVersionsTask.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Sundew.Build.Publish.Internal.Commands;
    using Sundew.Build.Publish.Internal.IO;
    using Sundew.Build.Publish.Internal.Logging;

    /// <summary>
    /// Task for adjusting project reference versions.
    /// </summary>
    public class AdjustProjectReferenceVersionsTask : Task
    {
        internal const string MSBuildSourceProjectFileName = "MSBuildSourceProjectFile";
        internal const string ProjectVersionName = "ProjectVersion";
        private const string SundewBuildPublishVersionFileExtension = "sbpv";
        private readonly IFileSystem fileSystem;
        private readonly ICommandLogger commandLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdjustProjectReferenceVersionsTask"/> class.
        /// </summary>
        public AdjustProjectReferenceVersionsTask()
            : this(new FileSystem(), null)
        {
        }

        internal AdjustProjectReferenceVersionsTask(IFileSystem fileSystem, ICommandLogger commandLogger)
        {
            this.fileSystem = fileSystem;
            this.commandLogger = commandLogger ?? new MsBuildCommandLogger(this.Log);
        }

        /// <summary>
        /// Gets or sets the resolved project references.
        /// </summary>
        /// <value>
        /// The resolved project references.
        /// </value>
        [Required]
        public ITaskItem[] ResolvedProjectReferences { get; set; }

        /// <summary>
        /// Gets or sets the project references.
        /// </summary>
        /// <value>
        /// The project references.
        /// </value>
        [Required]
        public ITaskItem[] ProjectReferences { get; set; }

        /// <summary>
        /// Gets the adjusted project references.
        /// </summary>
        /// <value>
        /// The adjusted project references.
        /// </value>
        [Output]
        public ITaskItem[] AdjustedProjectReferences { get; private set; }

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
                foreach (var projectReference in this.ProjectReferences)
                {
                    var resolvedProjectReference = this.ResolvedProjectReferences.FirstOrDefault(x =>
                        x.GetMetadata(MSBuildSourceProjectFileName) == projectReference.ItemSpec);

                    if (resolvedProjectReference == null)
                    {
                        continue;
                    }

                    var assemblyVersionFile = Path.ChangeExtension(resolvedProjectReference.ItemSpec, SundewBuildPublishVersionFileExtension);
                    if (this.fileSystem.FileExists(assemblyVersionFile))
                    {
                        var packageVersion = this.fileSystem.ReadAllText(assemblyVersionFile);
                        var referenceVersion = projectReference.GetMetadata(ProjectVersionName);
                        if (!string.IsNullOrEmpty(packageVersion) && !Equals(referenceVersion, packageVersion))
                        {
                            this.commandLogger.LogInfo($"Replaced version: {referenceVersion} with {packageVersion} for ProjectReference: {Path.GetFileName(projectReference.ItemSpec)} ");
                            projectReference.SetMetadata(ProjectVersionName, packageVersion);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.commandLogger.LogWarning(e.ToString());
            }

            this.AdjustedProjectReferences = this.ProjectReferences;
            return true;
        }
    }
}
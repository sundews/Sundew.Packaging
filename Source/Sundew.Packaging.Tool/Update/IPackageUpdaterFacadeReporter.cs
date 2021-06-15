// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageUpdaterFacadeReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Update
{
    using System;
    using System.Collections.Generic;
    using Sundew.Packaging.Tool.Reporting;
    using Sundew.Packaging.Tool.Update.MsBuild;

    public interface IPackageUpdaterFacadeReporter : IExceptionReporter
    {
        void StartingPackageUpdate(string rootDirectory);

        void UpdatingProject(string project);

        void CompletedPackageUpdate(List<MsBuildProject> msBuildProjects, bool argumentsSkipRestore, TimeSpan totalTime);
    }
}
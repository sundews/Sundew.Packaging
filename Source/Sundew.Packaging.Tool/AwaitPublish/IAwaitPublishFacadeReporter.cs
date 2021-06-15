// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAwaitPublishFacadeReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.AwaitPublish
{
    using System;
    using Sundew.Packaging.Tool.Reporting;
    using Sundew.Packaging.Tool.Update.MsBuild.NuGet;

    public interface IAwaitPublishFacadeReporter : IExceptionReporter
    {
        void StartWaitingForPackage(PackageIdAndVersion packageIdAndVersion, string source);

        void PackageExistsResult(PackageIdAndVersion packageIdAndVersion, bool packageExists);

        void CompletedWaitingForPackage(PackageIdAndVersion packageIdAndVersion, bool packageExists, TimeSpan stopwatchElapsed);
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPruneReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.PruneLocalSource
{
    using System;
    using System.Collections.Generic;
    using Sundew.Packaging.Tool.Reporting;

    public interface IPruneReporter : IExceptionReporter
    {
        void StartPruning(string source, IReadOnlyList<string> packageIds);

        void Deleted(string directory);

        void CompletedPruning(bool wasSuccessful, int numberDirectoriesPurged, TimeSpan stopwatchElapsed);
    }
}
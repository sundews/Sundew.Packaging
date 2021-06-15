// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDeleteFacadeReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Delete
{
    using System;
    using Sundew.Packaging.Tool.Reporting;

    public interface IDeleteFacadeReporter : IExceptionReporter
    {
        void StartingDelete(string rootedFileSpecification);

        void Deleted(string file);

        void CompletedDeleting(bool wasSuccessful, int numberFilesDeleted, TimeSpan stopwatchElapsed);
    }
}
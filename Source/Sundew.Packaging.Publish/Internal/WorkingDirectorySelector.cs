// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkingDirectorySelector.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    using System;
    using System.IO;
    using Sundew.Packaging.Publish.Internal.IO;

    internal static class WorkingDirectorySelector
    {
        private const string UndefinedText = "*Undefined*";

        public static string GetWorkingDirectory(string? proposedWorkingDirectory, IFileSystem fileSystem)
        {
            var workingDirectory = proposedWorkingDirectory;
            if (workingDirectory == UndefinedText)
            {
                workingDirectory = Path.GetDirectoryName(fileSystem.GetCurrentDirectory());
            }

            if (workingDirectory == null)
            {
                throw new ArgumentException("The working directory cannot be null.", nameof(workingDirectory));
            }

            return workingDirectory;
        }
    }
}
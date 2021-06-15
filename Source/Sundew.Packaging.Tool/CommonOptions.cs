// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonOptions.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool
{
    using System;
    using Sundew.CommandLine;

    /// <summary>
    /// Common command line options.
    /// </summary>
    public class CommonOptions
    {
        internal const string VersionGroupName = "Version";

        /// <summary>
        /// Adds the verbose.
        /// </summary>
        /// <param name="argumentsBuilder">The arguments builder.</param>
        /// <param name="verbose">if set to <c>true</c> [verbose].</param>
        /// <param name="setValue">The set value.</param>
        public static void AddVerbose(IArgumentsBuilder argumentsBuilder, bool verbose, Action<bool> setValue)
        {
            argumentsBuilder.AddSwitch("v", "verbose", verbose, setValue, "Verbose");
        }

        /// <summary>
        /// Adds the root directory.
        /// </summary>
        /// <param name="argumentsBuilder">The arguments builder.</param>
        /// <param name="serialize">The serialize.</param>
        /// <param name="deserialize">The deserialize.</param>
        public static void AddRootDirectory(IArgumentsBuilder argumentsBuilder, Func<string?> serialize, Action<string> deserialize)
        {
            argumentsBuilder.AddOptional("d", "root-directory", serialize, deserialize, "The directory to search for projects", true, defaultValueText: "Current directory");
        }
    }
}
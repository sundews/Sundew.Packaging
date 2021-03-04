// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAppendPublishFileLogCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands
{
    internal interface IAppendPublishFileLogCommand
    {
        /// <summary>
        /// Appends the specified output directory.
        /// </summary>
        /// <param name="workingDirectory">The output directory.</param>
        /// <param name="packagePushFileAppendFormats">The package push file append formats.</param>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="version">The version.</param>
        /// <param name="packagePath">The package path.</param>
        /// <param name="stage">The stage.</param>
        /// <param name="source">The source.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="feedSource">The feed source.</param>
        /// <param name="symbolPackagePath">The symbol package path.</param>
        /// <param name="symbolsSource">The symbols source.</param>
        /// <param name="symbolApiKey">The symbol API key.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="commandLogger">The command logger.</param>
        void Append(
            string workingDirectory,
            string packagePushFileAppendFormats,
            string packageId,
            string version,
            string packagePath,
            string stage,
            string source,
            string? apiKey,
            string feedSource,
            string? symbolPackagePath,
            string? symbolsSource,
            string? symbolApiKey,
            string parameter,
            ICommandLogger commandLogger);
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPushPackageCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Internal.Commands
{
    using global::NuGet.Common;
    using global::NuGet.Configuration;

    /// <summary>Interface for implementing a push command that pushes NuGet packages to a NuGet server.</summary>
    public interface IPushPackageCommand
    {
        /// <summary>Pushes the asynchronous.</summary>
        /// <param name="packagePath">The package path.</param>
        /// <param name="source">The source.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="symbolPackagePath">The symbol package path.</param>
        /// <param name="symbolsSource">The symbols source.</param>
        /// <param name="symbolApiKey">The symbol API key.</param>
        /// <param name="timeoutInSeconds">The timeout in seconds.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="noServiceEndpoint">The no service endpoint.</param>
        /// <param name="skipDuplicates">Skips duplicate.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="commandLogger">The command logger.</param>
        /// <returns>An async task.</returns>
        System.Threading.Tasks.Task PushAsync(
            string packagePath,
            string source,
            string apiKey,
            string symbolPackagePath,
            string symbolsSource,
            string symbolApiKey,
            int timeoutInSeconds,
            ISettings settings,
            bool noServiceEndpoint,
            bool skipDuplicates,
            ILogger logger,
            ICommandLogger commandLogger);
    }
}
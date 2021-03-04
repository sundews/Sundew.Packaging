// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PushPackageCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::NuGet.Commands;
    using global::NuGet.Common;
    using global::NuGet.Configuration;

    /// <summary>Pushes a NuGet package to a NuGet server.</summary>
    /// <seealso cref="IPushPackageCommand" />
    public class PushPackageCommand : IPushPackageCommand
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
        public async Task PushAsync(
            string packagePath,
            string? source,
            string? apiKey,
            string? symbolPackagePath,
            string? symbolsSource,
            string? symbolApiKey,
            int timeoutInSeconds,
            ISettings settings,
            bool noServiceEndpoint,
            bool skipDuplicates,
            ILogger logger,
            ICommandLogger commandLogger)
        {
            var packageSourceProvider = new PackageSourceProvider(settings);
            await PushRunner.Run(
                settings,
                packageSourceProvider,
                new List<string> { packagePath },
                source,
                apiKey,
                symbolsSource,
                symbolApiKey,
                timeoutInSeconds,
                false,
                string.IsNullOrEmpty(symbolPackagePath) || !string.IsNullOrEmpty(symbolsSource),
                noServiceEndpoint,
                skipDuplicates,
                logger);

            commandLogger.LogImportant($"Successfully pushed package to: {source}");
            if (!string.IsNullOrEmpty(symbolPackagePath) && symbolPackagePath != null && !string.IsNullOrEmpty(symbolsSource))
            {
                await PushRunner.Run(
                    settings,
                    packageSourceProvider,
                    new List<string> { symbolPackagePath },
                    symbolsSource,
                    symbolApiKey,
                    null,
                    null,
                    timeoutInSeconds,
                    false,
                    true,
                    noServiceEndpoint,
                    skipDuplicates,
                    logger);
                commandLogger.LogImportant($"Successfully pushed symbols package to: {symbolsSource}");
            }
        }
    }
}
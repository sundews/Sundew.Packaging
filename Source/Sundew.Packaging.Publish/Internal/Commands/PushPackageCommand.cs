// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PushPackageCommand.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands;

using System.Collections.Generic;
using System.Threading.Tasks;
using global::NuGet.Commands;
using global::NuGet.Configuration;
using Sundew.Packaging.Versioning.Logging;

/// <summary>Pushes a NuGet package to a NuGet server.</summary>
/// <seealso cref="IPushPackageCommand" />
public class PushPackageCommand : IPushPackageCommand
{
    private readonly ILogger logger;
    private readonly NuGet.Common.ILogger nuGetLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PushPackageCommand"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="nuGetLogger">The nu get logger.</param>
    public PushPackageCommand(
        global::Sundew.Packaging.Versioning.Logging.ILogger logger,
        global::NuGet.Common.ILogger nuGetLogger)
    {
        this.logger = logger;
        this.nuGetLogger = nuGetLogger;
    }

    /// <summary>
    /// Pushes the asynchronous.
    /// </summary>
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
    /// <returns>
    /// An async task.
    /// </returns>
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
        bool skipDuplicates)
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
            this.nuGetLogger);

        this.logger.LogImportant($"Successfully pushed package to: {source}");
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
                this.nuGetLogger);
            this.logger.LogImportant($"Successfully pushed symbols package to: {symbolsSource}");
        }
    }
}
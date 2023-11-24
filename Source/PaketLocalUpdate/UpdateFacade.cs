// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateFacade.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PaketLocalUpdate;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.FSharp.Core;
using NuGet.Configuration;
using Paket;
using Sundew.Packaging.RegularExpression;
using Sundew.Packaging.Versioning.Commands;

/// <summary>
/// A facade for performing paket update with a temporary local source.
/// </summary>
public class UpdateFacade
{
    private readonly INuGetSettingsInitializationCommand nuGetSettingsInitializationCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateFacade"/> class.
    /// </summary>
    /// <param name="nuGetSettingsInitializationCommand">The nu get settings initialization command.</param>
    public UpdateFacade(INuGetSettingsInitializationCommand nuGetSettingsInitializationCommand)
    {
        this.nuGetSettingsInitializationCommand = nuGetSettingsInitializationCommand;
    }

    /// <summary>
    /// Updates the specified arguments.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <returns>An async task.</returns>
    public async Task Update(Arguments arguments)
    {
        var workingDirectory = Directory.GetCurrentDirectory();
        var nuGetSettings = this.nuGetSettingsInitializationCommand.Initialize(workingDirectory, Sundew.Packaging.Source.PackageSources.DefaultLocalSourceName, Sundew.Packaging.Source.PackageSources.DefaultLocalSource);
        var source = nuGetSettings.PackageSourcesSection?.Items.OfType<AddItem>().FirstOrDefault(x => x.Key == arguments.Source)?.Value ?? arguments.Source;
        using var paketDependenciesTemporarySourceInjector = new PaketDependenciesTemporarySourceInjector(Dependencies.Locate(), new PaketDependenciesParser(), new FileSystemAsync());
        var (expression, isPattern) = GlobRegex.ConvertToRegexPattern(arguments.PackageId);
        var packageMatcher = arguments.IsFilter ? arguments.PackageId : expression;
        await paketDependenciesTemporarySourceInjector.Inject(source, packageMatcher, arguments.Group);
        Paket.Logging.@event.Publish.AddHandler(
            (_, args) =>
            {
                if (args.Level != TraceLevel.Verbose || arguments.IsVerbose)
                {
                    if (args.NewLine)
                    {
                        Console.WriteLine(args.Text);
                    }
                    else
                    {
                        Console.Write(args.Text);
                    }
                }
            });

        if (arguments.IsFilter || isPattern)
        {
            Paket.UpdateProcess.UpdateFilteredPackages(
                paketDependenciesTemporarySourceInjector.Dependencies.DependenciesFile,
                Domain.GroupName(arguments.Group),
                packageMatcher,
                FSharpOption<string>.None,
                new UpdaterOptions(InstallerOptions.Default, false));
        }
        else
        {
            Paket.UpdateProcess.UpdatePackage(
                paketDependenciesTemporarySourceInjector.Dependencies.DependenciesFile,
                Domain.GroupName(arguments.Group),
                Domain.PackageName(arguments.PackageId),
                FSharpOption<string>.None,
                new UpdaterOptions(InstallerOptions.Default, false));
        }
    }
}
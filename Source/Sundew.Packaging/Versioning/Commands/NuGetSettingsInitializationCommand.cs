// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetSettingsInitializationCommand.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.Commands;

using System.IO;
using System.Linq;
using global::NuGet.Configuration;
using Sundew.Packaging.Versioning.IO;
using Sundew.Packaging.Versioning.NuGet.Configuration;

/// <summary>Adds a local source to the specified NuGet.Config.</summary>
/// <seealso cref="INuGetSettingsInitializationCommand" />
public class NuGetSettingsInitializationCommand : INuGetSettingsInitializationCommand
{
    internal const string NuGetConfigFileName = "NuGet.Config";
    internal const string PackageSourcesText = "packageSources";

    private readonly ISettingsFactory settingsFactory;
    private readonly IFileSystem fileSystem;

    /// <summary>Initializes a new instance of the <see cref="NuGetSettingsInitializationCommand"/> class.</summary>
    public NuGetSettingsInitializationCommand()
        : this(new SettingsFactory(), new FileSystem())
    {
    }

    internal NuGetSettingsInitializationCommand(ISettingsFactory settingsFactory, IFileSystem fileSystem)
    {
        this.settingsFactory = settingsFactory;
        this.fileSystem = fileSystem;
    }

    /// <summary>Adds the specified local source to a NuGet.Config in solution dir.</summary>
    /// <param name="workingDirectory">The working directory.</param>
    /// <param name="localSourceName">The local source name.</param>
    /// <param name="localSource">The default local source.</param>
    /// <returns>The actual local source.</returns>
    public NuGetSettings Initialize(string workingDirectory, string localSourceName, string localSource)
    {
        var defaultSettings = this.settingsFactory.LoadDefaultSettings(workingDirectory);
        var packageSourcesSection = defaultSettings.GetSection(PackageSourcesText);
        var addItem = packageSourcesSection?.Items.OfType<AddItem>().FirstOrDefault(x => x.Key == localSourceName);
        if (addItem == null)
        {
            if (!this.fileSystem.DirectoryExists(localSource))
            {
                this.fileSystem.CreateDirectory(localSource);
            }

            var applicationDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            var applicationDataConfigurationFile = defaultSettings.GetConfigFilePaths().FirstOrDefault(x => x.StartsWith(applicationDataPath));
            if (applicationDataConfigurationFile != null)
            {
                var roamingSettings = this.settingsFactory.LoadSpecificSettings(Path.GetDirectoryName(applicationDataConfigurationFile), Path.GetFileName(applicationDataConfigurationFile));
                roamingSettings.AddOrUpdate(PackageSourcesText, new AddItem(localSourceName, localSource));
                roamingSettings.SaveToDisk();
            }

            return new NuGetSettings(localSource, defaultSettings, packageSourcesSection);
        }

        return new NuGetSettings(addItem.Value, defaultSettings, packageSourcesSection);
    }
}
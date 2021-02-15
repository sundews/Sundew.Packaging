// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetSettingsInitializationCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands
{
    using System.IO;
    using System.Linq;
    using global::NuGet.Configuration;
    using Sundew.Packaging.Publish.Internal.IO;
    using Sundew.Packaging.Publish.Internal.NuGet.Configuration;

    /// <summary>Adds a local source to the specified NuGet.Config.</summary>
    /// <seealso cref="INuGetSettingsInitializationCommand" />
    public class NuGetSettingsInitializationCommand : INuGetSettingsInitializationCommand
    {
        internal const string NuGetConfigFileName = "NuGet.Config";
        internal const string PackageSourcesText = "packageSources";

        private readonly IFileSystem fileSystem;
        private readonly ISettingsFactory settingsFactory;

        /// <summary>Initializes a new instance of the <see cref="NuGetSettingsInitializationCommand"/> class.</summary>
        public NuGetSettingsInitializationCommand()
            : this(new FileSystem(), new SettingsFactory())
        {
        }

        internal NuGetSettingsInitializationCommand(IFileSystem fileSystem, ISettingsFactory settingsFactory)
        {
            this.fileSystem = fileSystem;
            this.settingsFactory = settingsFactory;
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
                var settings = this.fileSystem.FileExists(Path.Combine(workingDirectory, NuGetConfigFileName))
                    ? this.settingsFactory.LoadSpecificSettings(workingDirectory, NuGetConfigFileName)
                    : this.settingsFactory.Create(workingDirectory, NuGetConfigFileName, false);
                settings.AddOrUpdate(PackageSourcesText, new AddItem(localSourceName, localSource));
                settings.SaveToDisk();
                return new NuGetSettings(localSource, defaultSettings, packageSourcesSection);
            }

            return new NuGetSettings(addItem.Value, defaultSettings, packageSourcesSection);
        }
    }
}
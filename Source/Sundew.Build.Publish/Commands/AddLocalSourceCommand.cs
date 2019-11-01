﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddLocalSourceCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Commands
{
    using System.IO;
    using System.Linq;
    using NuGet.Configuration;
    using Sundew.Build.Publish.Internal.IO;
    using Sundew.Build.Publish.Internal.NuGet.Configuration;

    /// <summary>Adds a local source to the specified NuGet.Config.</summary>
    /// <seealso cref="Sundew.Build.Publish.Commands.IAddLocalSourceCommand" />
    public class AddLocalSourceCommand : IAddLocalSourceCommand
    {
        internal const string NuGetConfigFileName = "NuGet.Config";
        internal const string PackageSourcesText = "packageSources";

        private readonly IFileSystem fileSystem;
        private readonly ISettingsFactory settingsFactory;

        /// <summary>Initializes a new instance of the <see cref="AddLocalSourceCommand"/> class.</summary>
        public AddLocalSourceCommand()
            : this(new FileSystem(), new SettingsFactory())
        {
        }

        internal AddLocalSourceCommand(IFileSystem fileSystem, ISettingsFactory settingsFactory)
        {
            this.fileSystem = fileSystem;
            this.settingsFactory = settingsFactory;
        }

        /// <summary>Adds the specified local source to a NuGet.Config in solution dir.</summary>
        /// <param name="solutionDir">The solution dir.</param>
        /// <param name="localSourceName">The local source name.</param>
        /// <param name="localSource">The default local source.</param>
        /// <returns>The actual local source.</returns>
        public LocalSource Add(string solutionDir, string localSourceName, string localSource)
        {
            var nugetConfigPath = Path.Combine(solutionDir, NuGetConfigFileName);
            var settings = this.fileSystem.FileExists(nugetConfigPath)
                ? this.settingsFactory.LoadSpecificSettings(solutionDir, NuGetConfigFileName)
                : this.settingsFactory.Create(solutionDir, NuGetConfigFileName, false);
            var defaultSettings = this.settingsFactory.LoadDefaultSettings(solutionDir);
            var packageSourcesSection = defaultSettings.GetSection(PackageSourcesText);
            var addItem = packageSourcesSection?.Items.OfType<AddItem>().FirstOrDefault(x => x.Key == localSourceName);
            if (addItem == null)
            {
                settings.AddOrUpdate(PackageSourcesText, new AddItem(localSourceName, localSource));
                settings.SaveToDisk();
                return new LocalSource(localSource, defaultSettings);
            }

            return new LocalSource(addItem.Value, defaultSettings);
        }
    }
}
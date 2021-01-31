// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddLocalSourceCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands
{
    using System;
    using System.IO;
    using System.Linq;
    using global::NuGet.Configuration;
    using Sundew.Packaging.Publish.Internal.IO;
    using Sundew.Packaging.Publish.Internal.NuGet.Configuration;

    /// <summary>Adds a local source to the specified NuGet.Config.</summary>
    /// <seealso cref="IAddLocalSourceCommand" />
    public class AddLocalSourceCommand : IAddLocalSourceCommand
    {
        internal const string NuGetConfigFileName = "NuGet.Config";
        internal const string PackageSourcesText = "packageSources";
        private const string UndefinedText = "*Undefined*";

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
        /// <param name="workingDirectory">The working directory.</param>
        /// <param name="localSourceName">The local source name.</param>
        /// <param name="localSource">The default local source.</param>
        /// <returns>The actual local source.</returns>
        public LocalSource Add(string? workingDirectory, string localSourceName, string localSource)
        {
            if (workingDirectory == UndefinedText)
            {
                workingDirectory = Path.GetDirectoryName(this.fileSystem.GetCurrentDirectory());
            }

            if (workingDirectory == null)
            {
                throw new ArgumentException("The working directory cannot be null.", nameof(workingDirectory));
            }

            var nugetConfigPath = Path.Combine(workingDirectory, NuGetConfigFileName);
            var defaultSettings = this.settingsFactory.LoadDefaultSettings(workingDirectory);
            var packageSourcesSection = defaultSettings.GetSection(PackageSourcesText);
            var addItem = packageSourcesSection?.Items.OfType<AddItem>().FirstOrDefault(x => x.Key == localSourceName);
            if (addItem == null)
            {
                var settings = this.fileSystem.FileExists(nugetConfigPath)
                    ? this.settingsFactory.LoadSpecificSettings(workingDirectory, NuGetConfigFileName)
                    : this.settingsFactory.Create(workingDirectory, NuGetConfigFileName, false);
                settings.AddOrUpdate(PackageSourcesText, new AddItem(localSourceName, localSource));
                settings.SaveToDisk();
                return new LocalSource(localSource, defaultSettings);
            }

            return new LocalSource(addItem.Value, defaultSettings);
        }
    }
}
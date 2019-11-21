// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalSource.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Internal.Commands
{
    using global::NuGet.Configuration;

    /// <summary>Contains information about local source and local symbols source.</summary>
    public readonly struct LocalSource
    {
        /// <summary>Initializes a new instance of the <see cref="LocalSource"/> struct.</summary>
        /// <param name="path">The path.</param>
        /// <param name="defaultSettings">The default settings.</param>
        public LocalSource(string path, ISettings defaultSettings)
        {
            this.Path = path;
            this.DefaultSettings = defaultSettings;
        }

        /// <summary>Gets the path.</summary>
        /// <value>The path.</value>
        public string Path { get; }

        /// <summary>Gets the default settings.</summary>
        /// <value>The default settings.</value>
        public ISettings DefaultSettings { get; }
    }
}
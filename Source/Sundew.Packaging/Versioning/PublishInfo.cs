// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublishInfo.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning
{
    /// <summary>
    /// Contains information for publishing NuGet packages.
    /// </summary>
    public class PublishInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishInfo" /> class.
        /// </summary>
        /// <param name="stage">The stage.</param>
        /// <param name="versionStage">The version stage.</param>
        /// <param name="feedSource">The feed source.</param>
        /// <param name="pushSource">The push source.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="symbolsPushSource">The symbols URI.</param>
        /// <param name="symbolsApiKey">The symbols API key.</param>
        /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
        /// <param name="version">The nuget version.</param>
        /// <param name="fullVersion">The full version.</param>
        /// <param name="metadata">The metadata.</param>
        public PublishInfo(
            string stage,
            string versionStage,
            string feedSource,
            string pushSource,
            string? apiKey,
            string? symbolsPushSource,
            string? symbolsApiKey,
            bool isEnabled,
            string version,
            string fullVersion,
            string? metadata)
        {
            this.Stage = stage;
            this.VersionStage = versionStage;
            this.FeedSource = feedSource;
            this.PushSource = pushSource;
            this.ApiKey = apiKey;
            this.SymbolsPushSource = symbolsPushSource;
            this.SymbolsApiKey = symbolsApiKey;
            this.IsEnabled = isEnabled;
            this.Version = version;
            this.FullVersion = fullVersion;
            this.Metadata = metadata;
        }

        /// <summary>
        /// Gets the stage.
        /// </summary>
        /// <value>
        /// The stage.
        /// </value>
        public string Stage { get; }

        /// <summary>
        /// Gets the version stage.
        /// </summary>
        /// <value>
        /// The stage.
        /// </value>
        public string VersionStage { get; }

        /// <summary>
        /// Gets the feed source.
        /// </summary>
        /// <value>
        /// The feed source.
        /// </value>
        public string FeedSource { get; }

        /// <summary>
        /// Gets the push source.
        /// </summary>
        /// <value>
        /// The push source.
        /// </value>
        public string PushSource { get; }

        /// <summary>
        /// Gets the API key.
        /// </summary>
        /// <value>
        /// The API key.
        /// </value>
        public string? ApiKey { get; }

        /// <summary>
        /// Gets the symbols URI.
        /// </summary>
        /// <value>
        /// The symbols URI.
        /// </value>
        public string? SymbolsPushSource { get; }

        /// <summary>
        /// Gets the symbols API key.
        /// </summary>
        /// <value>
        /// The symbols API key.
        /// </value>
        public string? SymbolsApiKey { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; }

        /// <summary>
        /// Gets the normalized version.
        /// </summary>
        /// <value>
        /// The normalized version.
        /// </value>
        public string Version { get; }

        /// <summary>
        /// Gets the full nuget version.
        /// </summary>
        /// <value>
        /// The nuget version.
        /// </value>
        public string FullVersion { get; }

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        /// <value>
        /// The metadata.
        /// </value>
        public string? Metadata { get; }
    }
}
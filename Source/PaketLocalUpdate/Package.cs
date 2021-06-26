// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Package.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PaketLocalUpdate
{
    /// <summary>
    /// Represents a paket package.
    /// </summary>
    public readonly struct Package
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Package"/> struct.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="prereleaseIndex">Index of the prerelease.</param>
        /// <param name="prereleaseLength">Length of the prerelease.</param>
        public Package(string id, int prereleaseIndex, int prereleaseLength)
        {
            this.Id = id;
            this.PrereleaseIndex = prereleaseIndex;
            this.PrereleaseLength = prereleaseLength;
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; }

        /// <summary>
        /// Gets the index of the prerelease.
        /// </summary>
        /// <value>
        /// The index of the prerelease.
        /// </value>
        public int PrereleaseIndex { get; }

        /// <summary>
        /// Gets the length of the prerelease.
        /// </summary>
        /// <value>
        /// The length of the prerelease.
        /// </value>
        public int PrereleaseLength { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is prerelease.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is prerelease; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrerelease => this.PrereleaseLength > 0;
    }
}
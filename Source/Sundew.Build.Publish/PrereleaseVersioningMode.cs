// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrereleaseVersioningMode.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish
{
    /// <summary>Determines how prereleases are versioned.</summary>
    public enum PrereleaseVersioningMode
    {
        /// <summary>Specifies that the patch component of the version number is incremented by 1, if the stable version exists.</summary>
        Automatic,

        /// <summary>Specifies that the patch component of the version number is incremented by 1.</summary>
        IncrementPatch,

        /// <summary>Specifies that the major, minor, patch version are not changed.</summary>
        NoChange,
    }
}
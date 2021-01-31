// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VersioningMode.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish
{
    /// <summary>Determines how packages are versioned.</summary>
    public enum VersioningMode
    {
        /// <summary>Specifies that the patch component of the version number is set to the latest incremented by 1.</summary>
        AutomaticLatestPatch,

        /// <summary>Specifies that stable builds are left as is and the patch component of the version number is incremented by 1 for prerelease, if the stable version exists.</summary>
        IncrementPatchIfStableExistForPrerelease,

        /// <summary>Specifies that the patch component of the version number is incremented by 1.</summary>
        AlwaysIncrementPatch,

        /// <summary>Specifies that the major, minor, patch version are not changed.</summary>
        NoChange,
    }
}
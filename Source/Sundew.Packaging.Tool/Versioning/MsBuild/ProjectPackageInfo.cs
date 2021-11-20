// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectPackageInfo.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Versioning.MsBuild;

/// <summary>
/// The id and version for a package.
/// </summary>
public record ProjectPackageInfo(string PackageId, string PackageVersion);
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageSources.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Source;

using System;
using System.IO;

/// <summary>
/// Constants for Sundew.Packacking apps.
/// </summary>
public static class PackageSources
{
    /// <summary>
    /// The default local source name.
    /// </summary>
    public const string DefaultLocalSourceName = "Local-SPP";

    /// <summary>
    /// The default local source.
    /// </summary>
    public static readonly string DefaultLocalSource = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sundew.Packaging.Publish"), "packages");
}
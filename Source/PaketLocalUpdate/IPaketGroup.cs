// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPaketGroup.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PaketLocalUpdate;

using System.Collections.Generic;

/// <summary>
/// A paket group containing sources and packages.
/// </summary>
public interface IPaketGroup
{
    /// <summary>
    /// Gets the sources.
    /// </summary>
    /// <value>
    /// The sources.
    /// </value>
    IReadOnlyList<Source> Sources { get; }

    /// <summary>
    /// Gets the packages.
    /// </summary>
    /// <value>
    /// The packages.
    /// </value>
    IReadOnlyList<Package> Packages { get; }
}
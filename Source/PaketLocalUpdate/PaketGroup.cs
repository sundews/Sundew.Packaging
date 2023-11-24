// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PaketGroup.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PaketLocalUpdate;

using System.Collections.Generic;

/// <summary>
/// Represents a paket group.
/// </summary>
/// <seealso cref="PaketLocalUpdate.IPaketGroup" />
public class PaketGroup : IPaketGroup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaketGroup"/> class.
    /// </summary>
    /// <param name="packages">The packages.</param>
    /// <param name="sources">The sources.</param>
    public PaketGroup(List<Package> packages, List<Source> sources)
    {
        this.Packages = packages;
        this.Sources = sources;
    }

    /// <summary>
    /// Gets the packages.
    /// </summary>
    /// <value>
    /// The packages.
    /// </value>
    public List<Package> Packages { get; }

    /// <summary>
    /// Gets the sources.
    /// </summary>
    /// <value>
    /// The sources.
    /// </value>
    public List<Source> Sources { get; }

    /// <summary>
    /// Gets the packages.
    /// </summary>
    /// <value>
    /// The packages.
    /// </value>
    IReadOnlyList<Package> IPaketGroup.Packages => this.Packages;

    /// <summary>
    /// Gets the sources.
    /// </summary>
    /// <value>
    /// The sources.
    /// </value>
    IReadOnlyList<Source> IPaketGroup.Sources => this.Sources;
}
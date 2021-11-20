// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Source.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PaketLocalUpdate;

/// <summary>
/// Represents a paket source.
/// </summary>
public readonly struct Source
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Source"/> struct.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="index">The index.</param>
    /// <param name="length">The length.</param>
    public Source(string path, int index, int length)
    {
        this.Path = path;
        this.Index = index;
        this.Length = length;
    }

    /// <summary>
    /// Gets the path.
    /// </summary>
    /// <value>
    /// The path.
    /// </value>
    public string Path { get; }

    /// <summary>
    /// Gets the index.
    /// </summary>
    /// <value>
    /// The index.
    /// </value>
    public int Index { get; }

    /// <summary>
    /// Gets the length.
    /// </summary>
    /// <value>
    /// The length.
    /// </value>
    public int Length { get; }
}
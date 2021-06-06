// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectedSource.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Source
{
    /// <summary>
    /// Represents the selected source in which to push a package to.
    /// </summary>
    /// <seealso cref="Sundew.Packaging.Source.Source" />
    /// <seealso cref="System.IEquatable{Sundew.Packaging.Source.Source}" />
    /// <seealso cref="System.IEquatable{Sundew.Packaging.Source.SelectedSource}" />
    public record SelectedSource : Source
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedSource"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="packagePrefix">The package prefix.</param>
        /// <param name="packagePostfix">The package postfix.</param>
        public SelectedSource(Source source, string? packagePrefix = null, string? packagePostfix = null)
          : base(source)
        {
            this.PackagePrefix = packagePrefix ?? string.Empty;
            this.PackagePostfix = packagePostfix ?? string.Empty;
        }

        /// <summary>
        /// Gets the package postfix.
        /// </summary>
        /// <value>
        /// The package postfix.
        /// </value>
        public string PackagePostfix { get; }

        /// <summary>
        /// Gets the package prefix.
        /// </summary>
        /// <value>
        /// The package prefix.
        /// </value>
        public string PackagePrefix { get; }
    }
}
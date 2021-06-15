// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectedStage.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Staging
{
    /// <summary>
    /// Represents the selected stage in which to push a package to.
    /// </summary>
    /// <seealso cref="Sundew.Packaging.Staging.Stage" />
    /// <seealso cref="System.IEquatable{Sundew.Packaging.Staging.Stage}" />
    /// <seealso cref="System.IEquatable{Sundew.Packaging.Staging.SelectedStage}" />
    public record SelectedStage : Stage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedStage"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="packagePrefix">The package prefix.</param>
        /// <param name="packagePostfix">The package postfix.</param>
        public SelectedStage(Stage source, string? packagePrefix = null, string? packagePostfix = null)
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
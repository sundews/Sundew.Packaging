// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Promotion.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Staging
{
    /// <summary>
    /// Determines the whether a stage was promoted.
    /// </summary>
    public enum Promotion
    {
        /// <summary>
        /// Indicates the stage type was selected by the stage.
        /// </summary>
        None,

        /// <summary>
        /// Indicates the stage type was selected by promotion.
        /// </summary>
        Promoted,
    }
}
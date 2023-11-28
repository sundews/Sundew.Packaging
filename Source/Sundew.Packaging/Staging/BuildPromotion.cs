// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuildPromotion.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Staging
{
    /// <summary>
    /// Determines the whether a build was promoted.
    /// </summary>
    public enum BuildPromotion
    {
        /// <summary>
        /// Indicates the stage was selected by the stage regex.
        /// </summary>
        None,

        /// <summary>
        /// Indicates the stage was selected by promotion.
        /// </summary>
        Promoted,
    }
}
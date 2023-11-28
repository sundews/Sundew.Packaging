// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuildPromotionInput.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Versioning
{
    using Sundew.Base.Text;
    using Sundew.Packaging.Versioning.IO;

    internal static class BuildPromotionInput
    {
        public static string? GetInput(
            string? buildPromotionInput,
            IFileSystem fileSystem)
        {
            if (buildPromotionInput.IsNullOrEmpty())
            {
                return null;
            }

            if (buildPromotionInput[0] == '<')
            {
                buildPromotionInput = fileSystem.ReadAllText(buildPromotionInput[1..].Trim());
            }

            return buildPromotionInput;
        }
    }
}
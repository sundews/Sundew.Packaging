// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StagePromotionMatcher.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Versioning
{
    using System.Text.RegularExpressions;
    using Sundew.Base.Text;
    using Sundew.Packaging.Staging;
    using Sundew.Packaging.Versioning.IO;

    public static class StagePromotionMatcher
    {
        public static StagePromotion GetStagePromotion(
            string? stagePromotionInput,
            string? stagePromotionRegex,
            IFileSystem fileSystem,
            IStageBuildLogger stageBuildLogger)
        {
            if (stagePromotionInput.IsNullOrEmpty() || stagePromotionRegex.IsNullOrEmpty())
            {
                return StagePromotion.None;
            }

            if (stagePromotionInput[0] == '<')
            {
                stagePromotionInput = fileSystem.ReadAllText(stagePromotionInput[1..].Trim());
            }

            var message = @$"Matching ""{stagePromotionInput}"" to ""{stagePromotionRegex}"" => result: ";
            var isPromoted = Regex.IsMatch(stagePromotionInput, stagePromotionRegex);
            stageBuildLogger.ReportMessage(message + (isPromoted ? "stage promoted" : "no promotion"));
            return isPromoted ? StagePromotion.Promoted : StagePromotion.None;
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobRegex.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.RegularExpression
{
    using System.Text.RegularExpressions;

    public sealed partial class GlobRegex : Regex
    {
        private GlobRegex(string pattern, string glob, RegexOptions regexOptions, bool isPattern)
          : base(pattern, regexOptions)
        {
            this.Pattern = pattern;
            this.Glob = glob;
            this.IsPattern = isPattern;
        }

        public string Pattern { get; }

        public string Glob { get; }

        public bool IsPattern { get; }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobRegex.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.RegularExpression
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// A regex for matching glob patterns.
    /// </summary>
    /// <seealso cref="System.Text.RegularExpressions.Regex" />
    public sealed partial class GlobRegex : Regex
    {
        private GlobRegex(string pattern, string glob, RegexOptions regexOptions, bool isPattern)
          : base(pattern, regexOptions)
        {
            this.Pattern = pattern;
            this.Glob = glob;
            this.IsPattern = isPattern;
        }

        /// <summary>
        /// Gets the pattern.
        /// </summary>
        /// <value>
        /// The pattern.
        /// </value>
        public string Pattern { get; }

        /// <summary>
        /// Gets the glob.
        /// </summary>
        /// <value>
        /// The glob.
        /// </value>
        public string Glob { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is pattern.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is pattern; otherwise, <c>false</c>.
        /// </value>
        public bool IsPattern { get; }
    }
}
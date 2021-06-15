// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobRegex.Factory.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.RegularExpression
{
    using System.Text.RegularExpressions;

    public sealed partial class GlobRegex
    {
        private const string Dot = ".";
        private const string RegexDot = @"\.";
        private const string WildcardAnyWithBackslash = @"**\";
        private const string WildcardAnyWithSlash = "**/";
        private const string WildcardAny = "**";
        private const string RegexWildcardAnyWithSlashes = ".*";
        private const string RegexWildcardAny = ".*(?:\\|/)";
        private const string WildcardAnyWithoutBackslash = "*";
        private const string RegexWildcardAnyWithoutSlashes = @"[^\\/]*";
        private const string WildcardOne = "?";
        private const string RegexWildcardOne = @"[^\\/]";
        private const string Slash = @"/";
        private const string Backslash = @"\";
        private const string RegexSlashes = @"(?:\\|/)";
        private const string GlobPattern = @"\*\*\\|\*\*/|\*\*|(?<NegativeRange>\[\!\.+\])|\.|\*|\?|\\|/";
        private const string NegativeRange = "NegativeRange";
        private const string Exclamation = "!";
        private const string Hat = "^";

        public static (string Expression, bool IsPattern) ConvertToRegexPattern(string pattern)
        {
            bool isPattern = false;
            var globRegexText = Regex.Replace(pattern, GlobPattern, match =>
            {
                isPattern = true;
                if (match.Groups[NegativeRange].Success)
                {
                    return match.Value.Replace(Exclamation, Hat);
                }

                return match.Value switch
                {
                    WildcardAny => RegexWildcardAny,
                    WildcardAnyWithBackslash => RegexWildcardAnyWithSlashes,
                    WildcardAnyWithSlash => RegexWildcardAnyWithSlashes,
                    WildcardAnyWithoutBackslash => RegexWildcardAnyWithoutSlashes,
                    WildcardOne => RegexWildcardOne,
                    Dot => RegexDot,
                    Backslash => RegexSlashes,
                    Slash => RegexSlashes,
                    _ => match.Value,
                };
            });

            return (globRegexText, isPattern);
        }

        public static GlobRegex Create(string pattern, bool matchLines = true, RegexOptions regexOptions = RegexOptions.None)
        {
            var (expression, isPattern) = ConvertToRegexPattern(pattern);
            if (matchLines)
            {
                return new($"^{expression}$", pattern, regexOptions, isPattern);
            }

            return new(expression, pattern, regexOptions, isPattern);
        }
    }
}
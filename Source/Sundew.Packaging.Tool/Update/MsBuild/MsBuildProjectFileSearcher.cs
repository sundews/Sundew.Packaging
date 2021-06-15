// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MsBuildProjectFileSearcher.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Update.MsBuild
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Sundew.Packaging.Tool.RegularExpression;

    public class MsBuildProjectFileSearcher
    {
        private const string AllFilesSearchPattern = "*";
        private const string MatchAllRegex = ".+";
        private static readonly IEnumerable<string> SearchPatterns = new[] { ".csproj", ".fsproj", ".vbproj" };
        private readonly IDirectory directory;

        public MsBuildProjectFileSearcher(IDirectory directory)
        {
            this.directory = directory;
        }

        public IEnumerable<string> GetProjects(string rootDirectory, IReadOnlyList<string> projects)
        {
            var projectRegexes = projects.Count == 0 ? new[] { new Regex(MatchAllRegex) } : projects.Select(x => GlobRegex.Create(x, false)).ToArray();
            return this.directory.EnumerateFiles(rootDirectory, AllFilesSearchPattern, SearchOption.AllDirectories)
                .Where(x => SearchPatterns.Any(x.EndsWith) && projectRegexes.Any(regex => regex.IsMatch(x)));
        }
    }
}
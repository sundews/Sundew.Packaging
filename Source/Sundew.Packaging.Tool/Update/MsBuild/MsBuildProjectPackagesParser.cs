// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MsBuildProjectPackagesParser.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Update.MsBuild
{
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using global::NuGet.Versioning;
    using Sundew.Packaging.RegularExpression;
    using Sundew.Packaging.Tool.Update.MsBuild.NuGet;

    public class MsBuildProjectPackagesParser
    {
        internal const string PackageReferenceRegex = @"(?<Prefix>\s*<PackageReference.+Include=\s*""(?<Id>{0})"".+Version=\s*"")(?<Version>[^""]+)(?<Postfix>"")";
        private const string IdGroupName = "Id";
        private const string VersionGroupName = "Version";
        private readonly IFile file;
        private readonly Dictionary<string, Regex> projectRegexCache = new();
        private readonly Dictionary<string, GlobRegex> versionRegexCache = new();

        public MsBuildProjectPackagesParser(IFile file)
        {
            this.file = file;
        }

        public async Task<MsBuildProject> GetPackages(string project, IReadOnlyList<PackageId> packageIds)
        {
            var projectContent = await this.file.ReadAllTextAsync(project);
            var absolutePackageIds = new List<PackageUpdateSuggestion>();
            foreach (var packageId in packageIds)
            {
                if (!this.projectRegexCache.TryGetValue(packageId.Id, out var projectRegex))
                {
                    var (expression, _) = GlobRegex.ConvertToRegexPattern(packageId.Id);
                    projectRegex = new Regex(string.Format(PackageReferenceRegex, expression));
                    this.projectRegexCache.Add(packageId.Id, projectRegex);
                }

                GlobRegex? versionRegex = null;
                if (!string.IsNullOrEmpty(packageId.VersionPattern) && !this.versionRegexCache.TryGetValue(packageId.VersionPattern, out versionRegex))
                {
                    versionRegex = GlobRegex.Create(packageId.VersionPattern);
                    this.versionRegexCache.Add(packageId.VersionPattern, versionRegex);
                }

                foreach (Match? match in projectRegex.Matches(projectContent))
                {
                    absolutePackageIds.Add(new PackageUpdateSuggestion(match!.Groups[IdGroupName].Value, NuGetVersion.Parse(match.Groups[VersionGroupName].Value), versionRegex));
                }
            }

            return new MsBuildProject(project, projectContent, absolutePackageIds);
        }
    }
}
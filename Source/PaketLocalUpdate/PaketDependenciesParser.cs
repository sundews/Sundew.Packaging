// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PaketDependenciesParser.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PaketLocalUpdate;

using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Paket.dependencies file parser.
/// </summary>
public class PaketDependenciesParser
{
    private const string Prerelease = "prerelease";
    private const string Package = "package";
    private const string Source = "source";
    private const string Group = "group";
    private const string Main = "Main";
    private static readonly Regex DependenciesRegex = new(@"^(?:(?: *group )(?<group>[\w]+)\s?|(?: *nuget )(?<package>\S+) *(?<prerelease>prerelease)?\S*\s?|(?<source> *source \S+)\s?)$", RegexOptions.Multiline | RegexOptions.ExplicitCapture);

    /// <summary>
    /// Parses the specified file content.
    /// </summary>
    /// <param name="fileContent">Content of the file.</param>
    /// <returns>A dictionary with the parsed paket groups.</returns>
    public IReadOnlyDictionary<string, PaketGroup> Parse(string fileContent)
    {
        var matches = DependenciesRegex.Matches(fileContent);
        var group = Main;
        var paketGroups = new Dictionary<string, PaketGroup>();
        foreach (Match? match in matches)
        {
            var groupGroup = match?.Groups[Group];
            if (groupGroup?.Success ?? false)
            {
                group = groupGroup.Value;
            }

            var sourceGroup = match?.Groups[Source];
            if (sourceGroup?.Success ?? false)
            {
                if (!paketGroups.TryGetValue(group, out var paketGroup))
                {
                    paketGroup = new PaketGroup(new List<Package>(), new List<Source>());
                    paketGroups.Add(group, paketGroup);
                }

                paketGroup.Sources.Add(new Source(sourceGroup.Value, sourceGroup.Index, sourceGroup.Length));
            }

            var packageGroup = match?.Groups[Package];
            if (packageGroup?.Success ?? false)
            {
                if (!paketGroups.TryGetValue(group, out var paketGroup))
                {
                    paketGroup = new PaketGroup(new List<Package>(), new List<Source>());
                    paketGroups.Add(group, paketGroup);
                }

                int prereleaseIndex;
                var prereleaseLength = 0;
                var prereleaseGroup = match?.Groups[Prerelease];
                if (prereleaseGroup?.Success ?? false)
                {
                    prereleaseIndex = prereleaseGroup.Index;
                    prereleaseLength = prereleaseGroup.Length;
                }
                else
                {
                    prereleaseIndex = packageGroup.Index + packageGroup.Length;
                }

                paketGroup.Packages.Add(new Package(packageGroup.Value, prereleaseIndex, prereleaseLength));
            }
        }

        return paketGroups;
    }
}
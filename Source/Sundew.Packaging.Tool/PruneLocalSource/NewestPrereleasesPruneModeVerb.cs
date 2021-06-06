// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewestPrereleasesPruneModeVerb.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.PruneLocalSource
{
    using System.Collections.Generic;
    using Sundew.CommandLine;
    using Sundew.Packaging.Source;

    public class NewestPrereleasesPruneModeVerb : IPruneModeVerb
    {
        private readonly List<string> packageIds;

        public NewestPrereleasesPruneModeVerb()
            : this(new List<string> { "*" }, PackageSources.DefaultLocalSourceName)
        {
        }

        public NewestPrereleasesPruneModeVerb(List<string> packageIds, string source, bool verbose = false)
        {
            this.packageIds = packageIds;
            this.Source = source;
            this.Verbose = verbose;
        }

        public string Source { get; private set; }

        public IReadOnlyList<string> PackageIds => this.packageIds;

        public bool Verbose { get; private set; }

        public IVerb? NextVerb { get; } = null;

        public string Name { get; } = "newest-prereleases";

        public string? ShortName { get; } = "np";

        public string HelpText { get; } = "Purges all prereleases never than the latest stable version.";

        public void Configure(IArgumentsBuilder argumentsBuilder)
        {
            argumentsBuilder.AddRequiredList("p", "package-ids", this.packageIds, "The packages to purge (* Wildcards supported)");
            argumentsBuilder.AddOptional("s", "source", () => this.Source, s => this.Source = s, @"Local source or source name to search for packages");
            CommonOptions.AddVerbose(argumentsBuilder, this.Verbose, b => this.Verbose = b);
        }
    }
}
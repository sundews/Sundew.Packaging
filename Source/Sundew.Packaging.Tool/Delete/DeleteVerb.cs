// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteVerb.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Delete
{
    using System.Collections.Generic;
    using Sundew.CommandLine;

    public class DeleteVerb : IVerb
    {
        private readonly List<string> files;

        public DeleteVerb()
          : this(new List<string>())
        {
        }

        public DeleteVerb(List<string> files, string? rootDirectory = null, bool recursive = false)
        {
            this.files = files;
            this.RootDirectory = rootDirectory;
            this.Recursive = recursive;
        }

        public IVerb? NextVerb { get; }

        public string Name { get; } = "delete";

        public string? ShortName { get; } = "d";

        public string HelpText { get; } = "Delete files";

        public IReadOnlyList<string> Files => this.files;

        public string? RootDirectory { get; private set; }

        public bool Recursive { get; private set; }

        public bool Verbose { get; private set; }

        public void Configure(IArgumentsBuilder argumentsBuilder)
        {
            argumentsBuilder.AddRequiredValues("files", this.files, "The files to be deleted", true);
            CommonOptions.AddRootDirectory(argumentsBuilder, () => this.RootDirectory, s => this.RootDirectory = s);
            argumentsBuilder.AddSwitch("r", "recursive", this.Recursive, b => this.Recursive = b, "Specifies whether to recurse into subdirectories.");
            CommonOptions.AddVerbose(argumentsBuilder, this.Verbose, b => this.Verbose = b);
        }
    }
}
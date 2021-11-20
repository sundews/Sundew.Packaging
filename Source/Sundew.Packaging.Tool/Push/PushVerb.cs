// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PushVerb.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Push;

using System.Collections.Generic;
using Sundew.CommandLine;

public class PushVerb : IVerb
{
    private readonly List<string> packagePaths;

    public PushVerb(List<string>? packagePaths = null, string? pushSource = null, string? apiKey = null)
        : this(packagePaths ?? new List<string>(), pushSource ?? string.Empty, apiKey ?? string.Empty, null)
    {
    }

    public PushVerb(List<string> packagePaths, string pushSource, string apiKey, string? symbolsPushSource = null, string? symbolsApiKey = null, string? workingDirectory = null, bool noSymbols = false, bool skipDuplicate = false, int timeoutSeconds = 300)
    {
        this.packagePaths = packagePaths;
        this.PushSource = pushSource;
        this.ApiKey = apiKey;
        this.SymbolsPushSource = symbolsPushSource;
        this.SymbolsApiKey = symbolsApiKey;
        this.WorkingDirectory = workingDirectory ?? string.Empty;
        this.NoSymbols = noSymbols;
        this.SkipDuplicate = skipDuplicate;
        this.TimeoutSeconds = timeoutSeconds;
    }

    public IVerb? NextVerb => null;

    public string Name { get; } = "push";

    public string? ShortName => null;

    public string HelpText => "Pushes the specified package(s) to a source";

    public string WorkingDirectory { get; private set; }

    public IReadOnlyList<string> PackagePaths => this.packagePaths;

    public string PushSource { get; private set; }

    public string ApiKey { get; private set; }

    public string? SymbolsPushSource { get; private set; }

    public string? SymbolsApiKey { get; private set; }

    public int TimeoutSeconds { get; private set; }

    public bool NoSymbols { get; private set; }

    public bool SkipDuplicate { get; private set; }

    public void Configure(IArgumentsBuilder argumentsBuilder)
    {
        argumentsBuilder.AddRequired("s", "source", () => this.PushSource, s => this.PushSource = s, "The source used to push packages.", false);
        argumentsBuilder.AddRequired("k", "api-key", () => this.ApiKey, s => this.ApiKey = s, "The api key to be used for the push.", false);
        argumentsBuilder.AddOptional("ss", "symbol-source", () => this.SymbolsPushSource, s => this.SymbolsPushSource = s, "The source used to push symbol packages.", false);
        argumentsBuilder.AddOptional("sk", "symbol-api-key", () => this.SymbolsApiKey, s => this.SymbolsApiKey = s, "The symbols api key used to push symbols.", false);
        argumentsBuilder.AddOptional("t", "timeout", () => this.TimeoutSeconds.ToString(), s => this.TimeoutSeconds = int.Parse(s), "Timeout for pushing to a source (seconds).");
        argumentsBuilder.AddSwitch("n", "no-symbols", this.NoSymbols, b => this.NoSymbols = b, "If set no symbols will be pushed.");
        argumentsBuilder.AddSwitch("sd", "skip-duplicate", this.SkipDuplicate, b => this.SkipDuplicate = b, "If a package already exists, skip it.");
        argumentsBuilder.AddRequiredValues("packages", this.packagePaths, "The packages to push (supports wildcards *).", true);
    }
}
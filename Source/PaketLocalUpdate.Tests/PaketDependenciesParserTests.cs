namespace PaketLocalUpdate.Tests;

using System.IO;
using PaketLocalUpdate;
using FluentAssertions;
using Sundew.Packaging.Source;
using Sundew.Packaging.Versioning.Commands;
using Xunit;


public class PaketDependenciesParserTests
{
    private readonly PaketDependenciesParser testee = new();

    [Fact]
    public void TestMethod1()
    {
        new NuGetSettingsInitializationCommand().Initialize(Directory.GetCurrentDirectory(), PackageSources.DefaultLocalSourceName, PackageSources.DefaultLocalSource);

        var fileContent = @"source https://nuget.org/api/v2

nuget Newtonsoft.Json
nuget UnionArgParser
nuget FSharp.Core

github forki/FsUnit FsUnit.fs
github fsharp/FAKE src/app/FakeLib/Globbing/Globbing.fs
github fsprojects/Chessie src/Chessie/ErrorHandling.fs

group Build

  source https://nuget.org/api/v2

  nuget FAKE
  nuget FSharp.Formatting
  nuget ILRepack

  github fsharp/FAKE modules/Octokit/Octokit.fsx

group Test

  source https://nuget.org/api/v2

  nuget NUnit.Runners prerelease
  nuget NUnit";

        var result = this.testee.Parse(fileContent);

        result.ToString();
    }
}
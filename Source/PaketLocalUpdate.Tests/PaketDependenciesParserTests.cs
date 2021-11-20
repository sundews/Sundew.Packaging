
using FluentAssertions;
using Xunit;

namespace PaketLocalUpdate.Tests;

public class PaketDependenciesParserTests
{
    private readonly PaketDependenciesParser testee;

    public PaketDependenciesParserTests()
    {
        this.testee = new PaketDependenciesParser();
    }
    [Fact]
    public void TestMethod1()
    {
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
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestData.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using global::NuGet.Versioning;
    using Sundew.Packaging.Tool.Update.MsBuild.NuGet;

    public static class TestData
    {
        public const string RootDirectory = @"C:\solutionDir";
        public static readonly ProjectTestData SundewBuildPublishProject = new(SundewPackagingPublishData.Name, SundewPackagingPublishData.Source, SundewPackagingPublishData.NonPrereleaseUpdatedSource, SundewPackagingPublishData.PrereleaseUpdatedSource);
        public static readonly ProjectTestData SundewCommandLineProject = new(SundewCommandLineData.Name, SundewCommandLineData.Source, SundewCommandLineData.NonPrereleaseUpdatedSource, SundewCommandLineData.PrereleaseUpdatedSource);
        public static readonly ProjectTestData TransparentMoqProject = new("TransparentMoq", TransparentMoqData.Source, TransparentMoqData.NonPrereleaseUpdatedSource, TransparentMoqData.Source);

        public static readonly PackageTestData SundewBuildPublishPackage = new(SundewPackagingPublishData.Name, SundewPackagingPublishData.PackageVersions);

        public static readonly PackageTestData SundewBasePackage = new(SundewBaseData.Name, SundewBaseData.PackageVersions);

        public static readonly PackageUpdate SundewBasePackageUpdateForSundewCommandLine = new(SundewBaseData.Name, NuGetVersion.Parse("5.0.0"), NuGetVersion.Parse("6.0.0"));

        public static readonly PackageUpdate SundewBasePackageUpdateForSundewPackagingPublish = new(SundewBaseData.Name, NuGetVersion.Parse("5.0.0"), NuGetVersion.Parse("5.1.1-pre002"));

        public static readonly PackageUpdate SundewBasePinnedPrereleasePackageUpdateForSundewCommandLine = new(SundewBaseData.Name, NuGetVersion.Parse("6.0.0"), NuGetVersion.Parse("5.1.1-pre002"));

        public static readonly PackageUpdate SundewBuildPublishPackageUpdateForSundewCommandLine = new(SundewPackagingPublishData.Name, NuGetVersion.Parse("0.0.1"), NuGetVersion.Parse("2.2.8"));

        public static readonly PackageUpdate SundewBasePrereleasePackageUpdateForSundewCommandLine = new(SundewBaseData.Name, NuGetVersion.Parse("6.0.0"), NuGetVersion.Parse("6.0.1-pre-u20191101-205349"));

        public static IEnumerable<PackageTestData> GetPackages()
        {
            yield return SundewBasePackage;
            yield return SundewBuildPublishPackage;
        }

        public static IEnumerable<ProjectTestData> GetProjects()
        {
            yield return SundewBuildPublishProject;
            yield return SundewCommandLineProject;
            yield return TransparentMoqProject;
        }

        public static void Assert<TTestData>(this TTestData testData, Action<TTestData> assertAction)
        {
            assertAction(testData);
        }

        public static string GetProjectPath(string project)
        {
            return Path.Combine(RootDirectory, project + ".csproj");
        }

        public static class SundewPackagingPublishData
        {
            public const string Name = "Sundew.Packaging.Publish";

            public const string Source = @"<Project TreatAsLocalProperty=""NodeReuse"" Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <MSBUILDDISABLENODEREUSE>1</MSBUILDDISABLENODEREUSE>
    <NodeReuse>False</NodeReuse>
    <LangVersion>9</LangVersion>
    <TargetFramework>netstandard2.0</TargetFramework>
    <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
    <NuspecFile>Sundew.Packaging.Publish.nuspec</NuspecFile>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
    <Authors>Kim Hugener-Ohlsen</Authors>
    <Company>Hukano</Company>
    <Owners>Kim Hugener-Ohlsen</Owners>
    <Description>
      Automated NuGet package publisher:
      - Publish prerelease package to local NuGet feed
      - Publish to official NuGet feed
      - Customizable publish for CI based on development, integration, production stages
      - Automated versioning of prereleases
      - Local debug support
    </Description>
    <RepositoryUrl>https://github.com/hugener/Sundew.Packaging.Publish</RepositoryUrl>
    <PackageReleaseNotes>
      2.0 - Added support for automatic prerelease patch increment
      1.0 - Initial release
    </PackageReleaseNotes>
    <RepositoryType>git</RepositoryType>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <Copyright>Kim Hugener-Ohlsen</Copyright>
    <Configurations>Debug;Release;Release-Stable</Configurations>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <DocumentationFile>$(OutputPath)\$(AssemblyName).xml</DocumentationFile>
    <PackageProjectUrl>https://github.com/hugener/Sundew.Packaging.Publish</PackageProjectUrl>
  </PropertyGroup>

  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Release-Stable|AnyCPU'"">
    <SbpSourceName>local-stable</SbpSourceName>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""ILRepack.Lib.MSBuild.Task"" Version=""2.0.18.2"" />
    <PackageReference Include=""Microsoft.Bcl.AsyncInterfaces"" Version=""5.0.0"" />
    <PackageReference Include=""Microsoft.Build.Utilities.Core"" Version=""16.8.0"" />
    <PackageReference Include=""NuGet.Commands"" Version=""5.8.0"" />
    <PackageReference Include=""NuGet.Protocol"" Version=""5.8.0"" />
    <PackageReference Include=""StyleCop.Analyzers"" Version=""1.1.118"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""Sundew.Base"" Version=""6.0.0"" />
    <PackageReference Include=""System.Reflection.Metadata"" Version=""5.0.0"" />
  </ItemGroup>

  <ItemGroup>
    <Compile Remove="".package\**"" />
    <EmbeddedResource Remove="".package\**"" />
    <None Remove="".package\**"" />
    <None Remove=""tools\**"" />
  </ItemGroup>

  <ItemGroup>
    <None Remove=""stylecop.json"" />
  </ItemGroup>

  <ItemGroup>
    <AdditionalFiles Include=""stylecop.json"" />
  </ItemGroup>

  <Target Name=""_DeletePackageDirectory"" AfterTargets=""PreBuildEvent"">
    <RemoveDir Directories="".package\tools"" ContinueOnError=""False"" />
  </Target>

  <Target Name=""SetNuspecProperties"" BeforeTargets=""GenerateNuspec"">
    <PropertyGroup>
      <NuspecProperties>$(NuspecProperties);company=$(Company);description=$(Description);copyright=$(Copyright);authors=$(Authors);version=$(PackageVersion);repositoryUrl=$(RepositoryUrl);releaseNotes=$(PackageReleaseNotes);repositoryType=$(RepositoryType);licenseExpression=$(PackageLicenseExpression);targetFramework=$(TargetFramework);configuration=$(Configuration);projectDir=$(MSBuildProjectDirectory);outputPath=$(OutputPath)</NuspecProperties>
    </PropertyGroup>
  </Target>

  <Target Name=""_CopyPackageFiles"" AfterTargets=""PostBuildEvent"">
    <ItemGroup>
      <OutputFiles Include=""$(OutputPath)\$(AssemblyName).m.dll;$(OutputPath)**\Microsoft.Build*.dll;$(OutputPath)**\NuGet*.dll;$(OutputPath)**\System*.dll"" />
      <RuntimeFiles Include=""..\NuGet.Runtime.Hack\bin\$(Configuration)\net5.0\runtimes\**\*.*"" />
      <TargetsFiles Include=""build\Sundew.Packaging.Publish.targets"" />
    </ItemGroup>

    <Copy SourceFiles=""@(OutputFiles)"" DestinationFiles=""@(OutputFiles->'.package\tools\%(RecursiveDir)%(Filename)%(Extension)')"" ContinueOnError=""False"" />
    <Copy SourceFiles=""@(RuntimeFiles)"" DestinationFiles=""@(RuntimeFiles->'.package\tools\runtimes\%(RecursiveDir)%(Filename)%(Extension)')"" ContinueOnError=""False"" />
    <Copy SourceFiles=""@(TargetsFiles)"" DestinationFiles=""@(TargetsFiles->'.package\build\%(RecursiveDir)%(Filename)%(Extension)')"" ContinueOnError=""False"" />
    <Delete Files="".package\tools\NuGet.Runtime.Hack.exe"" ContinueOnError=""False"" />
  </Target>

  <Target Name=""_CopySelf"" BeforeTargets=""Clean;Build"">
    <ItemGroup>
      <OutputSelfFiles Include=""$(OutputPath)\$(AssemblyName).m.dll;$(OutputPath)**\Microsoft.Build*.dll;$(OutputPath)**\NuGet*.dll;$(OutputPath)**\System*.dll"" />
    </ItemGroup>

    <Copy SourceFiles=""@(OutputSelfFiles)"" DestinationFiles=""@(OutputSelfFiles->'tools\%(RecursiveDir)%(Filename)%(Extension)')"" ContinueOnError=""False"" />
  </Target>

  <Import Project=""$(MSBuildProjectDirectory)\build\Sundew.Packaging.Publish.targets"" />
</Project>";

            public const string PrereleaseUpdatedSource = @"<Project TreatAsLocalProperty=""NodeReuse"" Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <MSBUILDDISABLENODEREUSE>1</MSBUILDDISABLENODEREUSE>
    <NodeReuse>False</NodeReuse>
    <LangVersion>9</LangVersion>
    <TargetFramework>netstandard2.0</TargetFramework>
    <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
    <NuspecFile>Sundew.Packaging.Publish.nuspec</NuspecFile>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
    <Authors>Kim Hugener-Ohlsen</Authors>
    <Company>Hukano</Company>
    <Owners>Kim Hugener-Ohlsen</Owners>
    <Description>
      Automated NuGet package publisher:
      - Publish prerelease package to local NuGet feed
      - Publish to official NuGet feed
      - Customizable publish for CI based on development, integration, production stages
      - Automated versioning of prereleases
      - Local debug support
    </Description>
    <RepositoryUrl>https://github.com/hugener/Sundew.Packaging.Publish</RepositoryUrl>
    <PackageReleaseNotes>
      2.0 - Added support for automatic prerelease patch increment
      1.0 - Initial release
    </PackageReleaseNotes>
    <RepositoryType>git</RepositoryType>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <Copyright>Kim Hugener-Ohlsen</Copyright>
    <Configurations>Debug;Release;Release-Stable</Configurations>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <DocumentationFile>$(OutputPath)\$(AssemblyName).xml</DocumentationFile>
    <PackageProjectUrl>https://github.com/hugener/Sundew.Packaging.Publish</PackageProjectUrl>
  </PropertyGroup>

  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Release-Stable|AnyCPU'"">
    <SbpSourceName>local-stable</SbpSourceName>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""ILRepack.Lib.MSBuild.Task"" Version=""2.0.18.2"" />
    <PackageReference Include=""Microsoft.Bcl.AsyncInterfaces"" Version=""5.0.0"" />
    <PackageReference Include=""Microsoft.Build.Utilities.Core"" Version=""16.8.0"" />
    <PackageReference Include=""NuGet.Commands"" Version=""5.8.0"" />
    <PackageReference Include=""NuGet.Protocol"" Version=""5.8.0"" />
    <PackageReference Include=""StyleCop.Analyzers"" Version=""1.1.118"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""Sundew.Base"" Version=""6.0.1-pre-u20191101-205349"" />
    <PackageReference Include=""System.Reflection.Metadata"" Version=""5.0.0"" />
  </ItemGroup>

  <ItemGroup>
    <Compile Remove="".package\**"" />
    <EmbeddedResource Remove="".package\**"" />
    <None Remove="".package\**"" />
    <None Remove=""tools\**"" />
  </ItemGroup>

  <ItemGroup>
    <None Remove=""stylecop.json"" />
  </ItemGroup>

  <ItemGroup>
    <AdditionalFiles Include=""stylecop.json"" />
  </ItemGroup>

  <Target Name=""_DeletePackageDirectory"" AfterTargets=""PreBuildEvent"">
    <RemoveDir Directories="".package\tools"" ContinueOnError=""False"" />
  </Target>

  <Target Name=""SetNuspecProperties"" BeforeTargets=""GenerateNuspec"">
    <PropertyGroup>
      <NuspecProperties>$(NuspecProperties);company=$(Company);description=$(Description);copyright=$(Copyright);authors=$(Authors);version=$(PackageVersion);repositoryUrl=$(RepositoryUrl);releaseNotes=$(PackageReleaseNotes);repositoryType=$(RepositoryType);licenseExpression=$(PackageLicenseExpression);targetFramework=$(TargetFramework);configuration=$(Configuration);projectDir=$(MSBuildProjectDirectory);outputPath=$(OutputPath)</NuspecProperties>
    </PropertyGroup>
  </Target>

  <Target Name=""_CopyPackageFiles"" AfterTargets=""PostBuildEvent"">
    <ItemGroup>
      <OutputFiles Include=""$(OutputPath)\$(AssemblyName).m.dll;$(OutputPath)**\Microsoft.Build*.dll;$(OutputPath)**\NuGet*.dll;$(OutputPath)**\System*.dll"" />
      <RuntimeFiles Include=""..\NuGet.Runtime.Hack\bin\$(Configuration)\net5.0\runtimes\**\*.*"" />
      <TargetsFiles Include=""build\Sundew.Packaging.Publish.targets"" />
    </ItemGroup>

    <Copy SourceFiles=""@(OutputFiles)"" DestinationFiles=""@(OutputFiles->'.package\tools\%(RecursiveDir)%(Filename)%(Extension)')"" ContinueOnError=""False"" />
    <Copy SourceFiles=""@(RuntimeFiles)"" DestinationFiles=""@(RuntimeFiles->'.package\tools\runtimes\%(RecursiveDir)%(Filename)%(Extension)')"" ContinueOnError=""False"" />
    <Copy SourceFiles=""@(TargetsFiles)"" DestinationFiles=""@(TargetsFiles->'.package\build\%(RecursiveDir)%(Filename)%(Extension)')"" ContinueOnError=""False"" />
    <Delete Files="".package\tools\NuGet.Runtime.Hack.exe"" ContinueOnError=""False"" />
  </Target>

  <Target Name=""_CopySelf"" BeforeTargets=""Clean;Build"">
    <ItemGroup>
      <OutputSelfFiles Include=""$(OutputPath)\$(AssemblyName).m.dll;$(OutputPath)**\Microsoft.Build*.dll;$(OutputPath)**\NuGet*.dll;$(OutputPath)**\System*.dll"" />
    </ItemGroup>

    <Copy SourceFiles=""@(OutputSelfFiles)"" DestinationFiles=""@(OutputSelfFiles->'tools\%(RecursiveDir)%(Filename)%(Extension)')"" ContinueOnError=""False"" />
  </Target>

  <Import Project=""$(MSBuildProjectDirectory)\build\Sundew.Packaging.Publish.targets"" />
</Project>";

            public const string NonPrereleaseUpdatedSource = @"<Project TreatAsLocalProperty=""NodeReuse"" Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <MSBUILDDISABLENODEREUSE>1</MSBUILDDISABLENODEREUSE>
    <NodeReuse>False</NodeReuse>
    <LangVersion>9</LangVersion>
    <TargetFramework>netstandard2.0</TargetFramework>
    <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
    <NuspecFile>Sundew.Packaging.Publish.nuspec</NuspecFile>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
    <Authors>Kim Hugener-Ohlsen</Authors>
    <Company>Hukano</Company>
    <Owners>Kim Hugener-Ohlsen</Owners>
    <Description>
      Automated NuGet package publisher:
      - Publish prerelease package to local NuGet feed
      - Publish to official NuGet feed
      - Customizable publish for CI based on development, integration, production stages
      - Automated versioning of prereleases
      - Local debug support
    </Description>
    <RepositoryUrl>https://github.com/hugener/Sundew.Packaging.Publish</RepositoryUrl>
    <PackageReleaseNotes>
      2.0 - Added support for automatic prerelease patch increment
      1.0 - Initial release
    </PackageReleaseNotes>
    <RepositoryType>git</RepositoryType>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <Copyright>Kim Hugener-Ohlsen</Copyright>
    <Configurations>Debug;Release;Release-Stable</Configurations>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <DocumentationFile>$(OutputPath)\$(AssemblyName).xml</DocumentationFile>
    <PackageProjectUrl>https://github.com/hugener/Sundew.Packaging.Publish</PackageProjectUrl>
  </PropertyGroup>

  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Release-Stable|AnyCPU'"">
    <SbpSourceName>local-stable</SbpSourceName>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""ILRepack.Lib.MSBuild.Task"" Version=""2.0.18.2"" />
    <PackageReference Include=""Microsoft.Bcl.AsyncInterfaces"" Version=""5.0.0"" />
    <PackageReference Include=""Microsoft.Build.Utilities.Core"" Version=""16.8.0"" />
    <PackageReference Include=""NuGet.Commands"" Version=""5.8.0"" />
    <PackageReference Include=""NuGet.Protocol"" Version=""5.8.0"" />
    <PackageReference Include=""StyleCop.Analyzers"" Version=""1.1.118"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""Sundew.Base"" Version=""6.0.0"" />
    <PackageReference Include=""System.Reflection.Metadata"" Version=""5.0.0"" />
  </ItemGroup>

  <ItemGroup>
    <Compile Remove="".package\**"" />
    <EmbeddedResource Remove="".package\**"" />
    <None Remove="".package\**"" />
    <None Remove=""tools\**"" />
  </ItemGroup>

  <ItemGroup>
    <None Remove=""stylecop.json"" />
  </ItemGroup>

  <ItemGroup>
    <AdditionalFiles Include=""stylecop.json"" />
  </ItemGroup>

  <Target Name=""_DeletePackageDirectory"" AfterTargets=""PreBuildEvent"">
    <RemoveDir Directories="".package\tools"" ContinueOnError=""False"" />
  </Target>

  <Target Name=""SetNuspecProperties"" BeforeTargets=""GenerateNuspec"">
    <PropertyGroup>
      <NuspecProperties>$(NuspecProperties);company=$(Company);description=$(Description);copyright=$(Copyright);authors=$(Authors);version=$(PackageVersion);repositoryUrl=$(RepositoryUrl);releaseNotes=$(PackageReleaseNotes);repositoryType=$(RepositoryType);licenseExpression=$(PackageLicenseExpression);targetFramework=$(TargetFramework);configuration=$(Configuration);projectDir=$(MSBuildProjectDirectory);outputPath=$(OutputPath)</NuspecProperties>
    </PropertyGroup>
  </Target>

  <Target Name=""_CopyPackageFiles"" AfterTargets=""PostBuildEvent"">
    <ItemGroup>
      <OutputFiles Include=""$(OutputPath)\$(AssemblyName).m.dll;$(OutputPath)**\Microsoft.Build*.dll;$(OutputPath)**\NuGet*.dll;$(OutputPath)**\System*.dll"" />
      <RuntimeFiles Include=""..\NuGet.Runtime.Hack\bin\$(Configuration)\net5.0\runtimes\**\*.*"" />
      <TargetsFiles Include=""build\Sundew.Packaging.Publish.targets"" />
    </ItemGroup>

    <Copy SourceFiles=""@(OutputFiles)"" DestinationFiles=""@(OutputFiles->'.package\tools\%(RecursiveDir)%(Filename)%(Extension)')"" ContinueOnError=""False"" />
    <Copy SourceFiles=""@(RuntimeFiles)"" DestinationFiles=""@(RuntimeFiles->'.package\tools\runtimes\%(RecursiveDir)%(Filename)%(Extension)')"" ContinueOnError=""False"" />
    <Copy SourceFiles=""@(TargetsFiles)"" DestinationFiles=""@(TargetsFiles->'.package\build\%(RecursiveDir)%(Filename)%(Extension)')"" ContinueOnError=""False"" />
    <Delete Files="".package\tools\NuGet.Runtime.Hack.exe"" ContinueOnError=""False"" />
  </Target>

  <Target Name=""_CopySelf"" BeforeTargets=""Clean;Build"">
    <ItemGroup>
      <OutputSelfFiles Include=""$(OutputPath)\$(AssemblyName).m.dll;$(OutputPath)**\Microsoft.Build*.dll;$(OutputPath)**\NuGet*.dll;$(OutputPath)**\System*.dll"" />
    </ItemGroup>

    <Copy SourceFiles=""@(OutputSelfFiles)"" DestinationFiles=""@(OutputSelfFiles->'tools\%(RecursiveDir)%(Filename)%(Extension)')"" ContinueOnError=""False"" />
  </Target>

  <Import Project=""$(MSBuildProjectDirectory)\build\Sundew.Packaging.Publish.targets"" />
</Project>";

            public const string MajorMinorPinnedSundewBaseUpdatedSource = @"<Project TreatAsLocalProperty=""NodeReuse"" Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <MSBUILDDISABLENODEREUSE>1</MSBUILDDISABLENODEREUSE>
    <NodeReuse>False</NodeReuse>
    <LangVersion>9</LangVersion>
    <TargetFramework>netstandard2.0</TargetFramework>
    <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
    <NuspecFile>Sundew.Packaging.Publish.nuspec</NuspecFile>
    <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
    <Authors>Kim Hugener-Ohlsen</Authors>
    <Company>Hukano</Company>
    <Owners>Kim Hugener-Ohlsen</Owners>
    <Description>
      Automated NuGet package publisher:
      - Publish prerelease package to local NuGet feed
      - Publish to official NuGet feed
      - Customizable publish for CI based on development, integration, production stages
      - Automated versioning of prereleases
      - Local debug support
    </Description>
    <RepositoryUrl>https://github.com/hugener/Sundew.Packaging.Publish</RepositoryUrl>
    <PackageReleaseNotes>
      2.0 - Added support for automatic prerelease patch increment
      1.0 - Initial release
    </PackageReleaseNotes>
    <RepositoryType>git</RepositoryType>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <Copyright>Kim Hugener-Ohlsen</Copyright>
    <Configurations>Debug;Release;Release-Stable</Configurations>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <DocumentationFile>$(OutputPath)\$(AssemblyName).xml</DocumentationFile>
    <PackageProjectUrl>https://github.com/hugener/Sundew.Packaging.Publish</PackageProjectUrl>
  </PropertyGroup>

  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Release-Stable|AnyCPU'"">
    <SbpSourceName>local-stable</SbpSourceName>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""ILRepack.Lib.MSBuild.Task"" Version=""2.0.18.2"" />
    <PackageReference Include=""Microsoft.Bcl.AsyncInterfaces"" Version=""5.0.0"" />
    <PackageReference Include=""Microsoft.Build.Utilities.Core"" Version=""16.8.0"" />
    <PackageReference Include=""NuGet.Commands"" Version=""5.8.0"" />
    <PackageReference Include=""NuGet.Protocol"" Version=""5.8.0"" />
    <PackageReference Include=""StyleCop.Analyzers"" Version=""1.1.118"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""Sundew.Base"" Version=""5.1.1-pre002"" />
    <PackageReference Include=""System.Reflection.Metadata"" Version=""5.0.0"" />
  </ItemGroup>

  <ItemGroup>
    <Compile Remove="".package\**"" />
    <EmbeddedResource Remove="".package\**"" />
    <None Remove="".package\**"" />
    <None Remove=""tools\**"" />
  </ItemGroup>

  <ItemGroup>
    <None Remove=""stylecop.json"" />
  </ItemGroup>

  <ItemGroup>
    <AdditionalFiles Include=""stylecop.json"" />
  </ItemGroup>

  <Target Name=""_DeletePackageDirectory"" AfterTargets=""PreBuildEvent"">
    <RemoveDir Directories="".package\tools"" ContinueOnError=""False"" />
  </Target>

  <Target Name=""SetNuspecProperties"" BeforeTargets=""GenerateNuspec"">
    <PropertyGroup>
      <NuspecProperties>$(NuspecProperties);company=$(Company);description=$(Description);copyright=$(Copyright);authors=$(Authors);version=$(PackageVersion);repositoryUrl=$(RepositoryUrl);releaseNotes=$(PackageReleaseNotes);repositoryType=$(RepositoryType);licenseExpression=$(PackageLicenseExpression);targetFramework=$(TargetFramework);configuration=$(Configuration);projectDir=$(MSBuildProjectDirectory);outputPath=$(OutputPath)</NuspecProperties>
    </PropertyGroup>
  </Target>

  <Target Name=""_CopyPackageFiles"" AfterTargets=""PostBuildEvent"">
    <ItemGroup>
      <OutputFiles Include=""$(OutputPath)\$(AssemblyName).m.dll;$(OutputPath)**\Microsoft.Build*.dll;$(OutputPath)**\NuGet*.dll;$(OutputPath)**\System*.dll"" />
      <RuntimeFiles Include=""..\NuGet.Runtime.Hack\bin\$(Configuration)\net5.0\runtimes\**\*.*"" />
      <TargetsFiles Include=""build\Sundew.Packaging.Publish.targets"" />
    </ItemGroup>

    <Copy SourceFiles=""@(OutputFiles)"" DestinationFiles=""@(OutputFiles->'.package\tools\%(RecursiveDir)%(Filename)%(Extension)')"" ContinueOnError=""False"" />
    <Copy SourceFiles=""@(RuntimeFiles)"" DestinationFiles=""@(RuntimeFiles->'.package\tools\runtimes\%(RecursiveDir)%(Filename)%(Extension)')"" ContinueOnError=""False"" />
    <Copy SourceFiles=""@(TargetsFiles)"" DestinationFiles=""@(TargetsFiles->'.package\build\%(RecursiveDir)%(Filename)%(Extension)')"" ContinueOnError=""False"" />
    <Delete Files="".package\tools\NuGet.Runtime.Hack.exe"" ContinueOnError=""False"" />
  </Target>

  <Target Name=""_CopySelf"" BeforeTargets=""Clean;Build"">
    <ItemGroup>
      <OutputSelfFiles Include=""$(OutputPath)\$(AssemblyName).m.dll;$(OutputPath)**\Microsoft.Build*.dll;$(OutputPath)**\NuGet*.dll;$(OutputPath)**\System*.dll"" />
    </ItemGroup>

    <Copy SourceFiles=""@(OutputSelfFiles)"" DestinationFiles=""@(OutputSelfFiles->'tools\%(RecursiveDir)%(Filename)%(Extension)')"" ContinueOnError=""False"" />
  </Target>

  <Import Project=""$(MSBuildProjectDirectory)\build\Sundew.Packaging.Publish.targets"" />
</Project>";

            public static readonly NuGetVersion[] PackageVersions = new[]
            {
                NuGetVersion.Parse("0.0.2-pre-u20191101-205349"),
                NuGetVersion.Parse("2.2.8"),
            };
        }

        public static class SundewCommandLineData
        {
            public const string Name = "Sundew.CommandLine";

            public const string Source = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netstandard1.3</TargetFramework>
	  <LangVersion>8</LangVersion>
    <Nullable>enable</Nullable>
    <Authors>Kim Hugener-Ohlsen</Authors>
    <Company>Hukano</Company>
    <DocumentationFile>$(OutputPath)/$(AssemblyName).xml</DocumentationFile>
    <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
    <Description>Simple library for creating a command line.
Features:
- *nix style command line. -x, --xx
- Includes command line parser, generator and help generator
- Object oriented
- Verbs and non-verbs (Implement IVerb or IArguments)
- Parses/Generates: simple types, lists, nested types.
- Supports optional/required arguments
- Nested arguments for argument grouping and reuse</Description>
    <PackageReleaseNotes>4.2 - Added Parser Result extensions and help text improvements
4.1 - Async support
4.0 - Support nested verbs
3.0 - Improved nesting support</PackageReleaseNotes>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <RepositoryUrl>https://github.com/hugener/Sundew.CommandLine</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <Copyright>Kim Hugener-Ohlsen</Copyright>
	  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <TreatSpecificWarningsAsErrors />
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <WarningsAsErrors />
    <NoWarn>SA1625</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <None Remove=""stylecop.json"" />
  </ItemGroup>

  <ItemGroup>
    <AdditionalFiles Include=""stylecop.json"" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.CodeAnalysis.FxCopAnalyzers"" Version=""3.3.0"" >
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""Nullable"" Version=""1.3.0"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""StyleCop.Analyzers"" Version=""1.2.0-beta.321"" >
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""Sundew.Base"" Version=""5.0.0"" />
    <PackageReference Include= ""Sundew.Packaging.Publish"" Version=""0.0.1"" >
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""System.ValueTuple"" Version=""4.5.0"" />
  </ItemGroup>

</Project>";

            public const string PrereleaseUpdatedSource = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netstandard1.3</TargetFramework>
	  <LangVersion>8</LangVersion>
    <Nullable>enable</Nullable>
    <Authors>Kim Hugener-Ohlsen</Authors>
    <Company>Hukano</Company>
    <DocumentationFile>$(OutputPath)/$(AssemblyName).xml</DocumentationFile>
    <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
    <Description>Simple library for creating a command line.
Features:
- *nix style command line. -x, --xx
- Includes command line parser, generator and help generator
- Object oriented
- Verbs and non-verbs (Implement IVerb or IArguments)
- Parses/Generates: simple types, lists, nested types.
- Supports optional/required arguments
- Nested arguments for argument grouping and reuse</Description>
    <PackageReleaseNotes>4.2 - Added Parser Result extensions and help text improvements
4.1 - Async support
4.0 - Support nested verbs
3.0 - Improved nesting support</PackageReleaseNotes>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <RepositoryUrl>https://github.com/hugener/Sundew.CommandLine</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <Copyright>Kim Hugener-Ohlsen</Copyright>
	  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <TreatSpecificWarningsAsErrors />
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <WarningsAsErrors />
    <NoWarn>SA1625</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <None Remove=""stylecop.json"" />
  </ItemGroup>

  <ItemGroup>
    <AdditionalFiles Include=""stylecop.json"" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.CodeAnalysis.FxCopAnalyzers"" Version=""3.3.0"" >
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""Nullable"" Version=""1.3.0"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""StyleCop.Analyzers"" Version=""1.2.0-beta.321"" >
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""Sundew.Base"" Version=""6.0.1-pre-u20191101-205349"" />
    <PackageReference Include= ""Sundew.Packaging.Publish"" Version=""2.2.8"" >
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""System.ValueTuple"" Version=""4.5.0"" />
  </ItemGroup>

</Project>";

            public const string NonPrereleaseUpdatedSource = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netstandard1.3</TargetFramework>
	  <LangVersion>8</LangVersion>
    <Nullable>enable</Nullable>
    <Authors>Kim Hugener-Ohlsen</Authors>
    <Company>Hukano</Company>
    <DocumentationFile>$(OutputPath)/$(AssemblyName).xml</DocumentationFile>
    <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
    <Description>Simple library for creating a command line.
Features:
- *nix style command line. -x, --xx
- Includes command line parser, generator and help generator
- Object oriented
- Verbs and non-verbs (Implement IVerb or IArguments)
- Parses/Generates: simple types, lists, nested types.
- Supports optional/required arguments
- Nested arguments for argument grouping and reuse</Description>
    <PackageReleaseNotes>4.2 - Added Parser Result extensions and help text improvements
4.1 - Async support
4.0 - Support nested verbs
3.0 - Improved nesting support</PackageReleaseNotes>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <RepositoryUrl>https://github.com/hugener/Sundew.CommandLine</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <Copyright>Kim Hugener-Ohlsen</Copyright>
	  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <TreatSpecificWarningsAsErrors />
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <WarningsAsErrors />
    <NoWarn>SA1625</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <None Remove=""stylecop.json"" />
  </ItemGroup>

  <ItemGroup>
    <AdditionalFiles Include=""stylecop.json"" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.CodeAnalysis.FxCopAnalyzers"" Version=""3.3.0"" >
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""Nullable"" Version=""1.3.0"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""StyleCop.Analyzers"" Version=""1.2.0-beta.321"" >
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""Sundew.Base"" Version=""6.0.0"" />
    <PackageReference Include= ""Sundew.Packaging.Publish"" Version=""2.2.8"" >
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""System.ValueTuple"" Version=""4.5.0"" />
  </ItemGroup>

</Project>";

            public const string PinnedSundewBaseUpdatedSource = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netstandard1.3</TargetFramework>
	  <LangVersion>8</LangVersion>
    <Nullable>enable</Nullable>
    <Authors>Kim Hugener-Ohlsen</Authors>
    <Company>Hukano</Company>
    <DocumentationFile>$(OutputPath)/$(AssemblyName).xml</DocumentationFile>
    <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
    <Description>Simple library for creating a command line.
Features:
- *nix style command line. -x, --xx
- Includes command line parser, generator and help generator
- Object oriented
- Verbs and non-verbs (Implement IVerb or IArguments)
- Parses/Generates: simple types, lists, nested types.
- Supports optional/required arguments
- Nested arguments for argument grouping and reuse</Description>
    <PackageReleaseNotes>4.2 - Added Parser Result extensions and help text improvements
4.1 - Async support
4.0 - Support nested verbs
3.0 - Improved nesting support</PackageReleaseNotes>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <RepositoryUrl>https://github.com/hugener/Sundew.CommandLine</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <Copyright>Kim Hugener-Ohlsen</Copyright>
	  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <TreatSpecificWarningsAsErrors />
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <WarningsAsErrors />
    <NoWarn>SA1625</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <None Remove=""stylecop.json"" />
  </ItemGroup>

  <ItemGroup>
    <AdditionalFiles Include=""stylecop.json"" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.CodeAnalysis.FxCopAnalyzers"" Version=""3.3.0"" >
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""Nullable"" Version=""1.3.0"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""StyleCop.Analyzers"" Version=""1.2.0-beta.321"" >
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""Sundew.Base"" Version=""6.0.0"" />
    <PackageReference Include= ""Sundew.Packaging.Publish"" Version=""0.0.1"" >
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""System.ValueTuple"" Version=""4.5.0"" />
  </ItemGroup>

</Project>";

            public const string MajorMinorPinnedSundewBaseUpdatedSource = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netstandard1.3</TargetFramework>
	  <LangVersion>8</LangVersion>
    <Nullable>enable</Nullable>
    <Authors>Kim Hugener-Ohlsen</Authors>
    <Company>Hukano</Company>
    <DocumentationFile>$(OutputPath)/$(AssemblyName).xml</DocumentationFile>
    <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
    <Description>Simple library for creating a command line.
Features:
- *nix style command line. -x, --xx
- Includes command line parser, generator and help generator
- Object oriented
- Verbs and non-verbs (Implement IVerb or IArguments)
- Parses/Generates: simple types, lists, nested types.
- Supports optional/required arguments
- Nested arguments for argument grouping and reuse</Description>
    <PackageReleaseNotes>4.2 - Added Parser Result extensions and help text improvements
4.1 - Async support
4.0 - Support nested verbs
3.0 - Improved nesting support</PackageReleaseNotes>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <RepositoryUrl>https://github.com/hugener/Sundew.CommandLine</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <Copyright>Kim Hugener-Ohlsen</Copyright>
	  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <TreatSpecificWarningsAsErrors />
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <WarningsAsErrors />
    <NoWarn>SA1625</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <None Remove=""stylecop.json"" />
  </ItemGroup>

  <ItemGroup>
    <AdditionalFiles Include=""stylecop.json"" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.CodeAnalysis.FxCopAnalyzers"" Version=""3.3.0"" >
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""Nullable"" Version=""1.3.0"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""StyleCop.Analyzers"" Version=""1.2.0-beta.321"" >
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""Sundew.Base"" Version=""5.1.1-pre002"" />
    <PackageReference Include= ""Sundew.Packaging.Publish"" Version=""0.0.1"" >
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include=""System.ValueTuple"" Version=""4.5.0"" />
  </ItemGroup>

</Project>";
        }

        public static class TransparentMoqData
        {
            public const string Source = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFrameworks>net45;netstandard2.0;netstandard2.1</TargetFrameworks>
    <Authors>Kim Hugener-Ohlsen</Authors>
    <Company>Hukano</Company>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageReleaseNotes>4.16.0 - https://github.com/moq/moq4/releases/tag/v4.16.0
4.15.2 - https://github.com/moq/moq4/releases/tag/v4.15.2
1.0 - Initial release
    </PackageReleaseNotes>
    <RepositoryType>git</RepositoryType>
    <PackageProjectUrl>https://github.com/hugener/TransparentMoq</PackageProjectUrl>
    <RepositoryUrl>https://github.com/hugener/TransparentMoq</RepositoryUrl>
    <Copyright>Kim Hugener-Ohlsen</Copyright>
    <Description>
With inspiration taken from mocking frameworks like NSubstitute, FakeItEasy and JustMock this package adds some of the syntactic advantages of these frameworks to Moq.

TransparentMoq allows to use Moq without having to store mocks in Mock&lt;T&gt; variables, instead a T variable can be used. This also removes the need to pass mocks with "".Object"" everywhere as they can be passed directly.

The library provides extension methods for most (if not all) of Moq's methods, so they can be called directly on the T variable.
Examples:
Instead of
private Mock&lt;IFileSystem&gt; fileSystem = new Mock&lt;IFileSystem&gt;();
Write
private IFileSystem fileSystem = New.Mock&lt;IFileSystem&gt;();

Instead of
MethodThatTakesFileSystem(fileSystem.Object);
write
MethodThatTakesFileSystem(fileSystem);

To arrange a mock everything remains the same:
fileSystem.Setup(x =&gt; x.Exists(It.IsAny&lt;string&gt;())).Returns(true);</Description>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageTags>moq;tdd;mocking;mocks;unittesting;agile;unittest;transparentmoq</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <Compile Remove=""TransparentMoq\**"" />
    <EmbeddedResource Remove=""TransparentMoq\**"" />
    <None Remove=""TransparentMoq\**"" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include=""Moq"" Version=""4.16.0"" />
    <PackageReference Include=""Sundew.Packaging.Publish"" Version=""2.2.7"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

</Project>
";

            public const string NonPrereleaseUpdatedSource = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFrameworks>net45;netstandard2.0;netstandard2.1</TargetFrameworks>
    <Authors>Kim Hugener-Ohlsen</Authors>
    <Company>Hukano</Company>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageReleaseNotes>4.16.0 - https://github.com/moq/moq4/releases/tag/v4.16.0
4.15.2 - https://github.com/moq/moq4/releases/tag/v4.15.2
1.0 - Initial release
    </PackageReleaseNotes>
    <RepositoryType>git</RepositoryType>
    <PackageProjectUrl>https://github.com/hugener/TransparentMoq</PackageProjectUrl>
    <RepositoryUrl>https://github.com/hugener/TransparentMoq</RepositoryUrl>
    <Copyright>Kim Hugener-Ohlsen</Copyright>
    <Description>
With inspiration taken from mocking frameworks like NSubstitute, FakeItEasy and JustMock this package adds some of the syntactic advantages of these frameworks to Moq.

TransparentMoq allows to use Moq without having to store mocks in Mock&lt;T&gt; variables, instead a T variable can be used. This also removes the need to pass mocks with "".Object"" everywhere as they can be passed directly.

The library provides extension methods for most (if not all) of Moq's methods, so they can be called directly on the T variable.
Examples:
Instead of
private Mock&lt;IFileSystem&gt; fileSystem = new Mock&lt;IFileSystem&gt;();
Write
private IFileSystem fileSystem = New.Mock&lt;IFileSystem&gt;();

Instead of
MethodThatTakesFileSystem(fileSystem.Object);
write
MethodThatTakesFileSystem(fileSystem);

To arrange a mock everything remains the same:
fileSystem.Setup(x =&gt; x.Exists(It.IsAny&lt;string&gt;())).Returns(true);</Description>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageTags>moq;tdd;mocking;mocks;unittesting;agile;unittest;transparentmoq</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <Compile Remove=""TransparentMoq\**"" />
    <EmbeddedResource Remove=""TransparentMoq\**"" />
    <None Remove=""TransparentMoq\**"" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include=""Moq"" Version=""4.16.0"" />
    <PackageReference Include=""Sundew.Packaging.Publish"" Version=""2.2.8"">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

</Project>
";

            public static readonly NuGetVersion[] PackageVersions = new[]
            {
                NuGetVersion.Parse("4.15.2"),
                NuGetVersion.Parse("4.16.0"),
                NuGetVersion.Parse("1.2.1"),
            };
        }

        public static class SundewBaseData
        {
            public const string Name = "Sundew.Base";

            public static readonly NuGetVersion[] PackageVersions = new[]
            {
                NuGetVersion.Parse("4.0.0-pre002"),
                NuGetVersion.Parse("4.0.0"),
                NuGetVersion.Parse("5.1.0"),
                NuGetVersion.Parse("5.1.1-pre002"),
                NuGetVersion.Parse("6.0.0"),
                NuGetVersion.Parse("6.0.1-pre-u20191101-205349"),
            };
        }
    }

    public record ProjectTestData(string Name, string Source, string ExpectedNonPrereleaseUpdatedSource, string ExpectedPrereleaseUpdatedSource)
    {
        public string Path => TestData.GetProjectPath(this.Name);
    }

    public record PackageTestData(string Id, NuGetVersion[] Versions);
}
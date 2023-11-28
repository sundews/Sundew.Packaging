# Sundew.Packaging

## **Sundew.Packaging.Publish**

## **1. Description**
Sundew.Packaging.Publish is an automatic package publisher that can work standalone locally or integrated as a part of a CI/CD pipeline.
- Automation for multi-repos
- Publish prerelease package to local NuGet feed
- Publish to official NuGet feed (or default push source)
- Customizable publish for CI based on development, integration, production stages
- Automated patch and prerelease versioning
- Local debug support
- Local Paket support with PaketLocalUpdate tool: 

## **2. Setup**
Sundew.Packaging.Publish is available on NuGet at: https://www.nuget.org/packages/Sundew.Packaging.Publish

### **2.1 Disabling Sundew.Packaging.Publish**
There are two ways to disable the automatic publishing.
- Define constant **DISABLE_SPP** - Disables Sundew.Packaging.Publish entirely
- Set MSBuild property **SppEnablePublish** to false - Disables publish
- Define constant **SPP_DISABLE_PUBLISH** - Same as above

## **3. Local builds**
Local builds are NuGet packages that are deployed to the local machine. This allows the packages to be consumed instantenously by other projects.

### **3.1 Setting up local builds**
Sundew.Packaging.Publish produced local builds and outputs them to the default local source: **(%LocalAppData%\Sundew.Packaging.Publish\packages)**.
The are a number of ways to enable local builds
1. Place an empty SppWorkLocally.user in the solution directory (Should not be checked in).
2. Set the MSBuild property SppWorkLocally to true in a .props.user file (Should not be checked in).
3. If GeneratePackageOnBuild is explicitly set to true, local builds are enabled.

### **3.2 Local source from NuGet.config**
NuGet.config provides a hierarical way of configuring NuGet from the local solution folder to the machinewide settings.  
During build, the used local source will be added to the ApplicationData specific NuGet.config with the name: **Local-SPP** (If the name does not already exist).  
This allows the default local source to be overwritten by adding a package source named **Local-SPP**.  
Use the NuGet commandline to modify the settings: https://docs.microsoft.com/en-us/nuget/consume-packages/configuring-nuget-behavior

### **3.3 Local source from project**
The local source can also be overwritten by settings the **SppLocalSource** MSBuild property.

### **3.4 Stable local builds**
Local build can be created with a stable version number by setting the  **SppSourceName** MSBuild property to: **local-stable**

### **3.5 Publishing local builds**
Local builds can also be published by setting the **SppSourceName** MSBuild property.  
Valid values are:
- **default** - Creates a prerelease build and publishes it to the default push source.
- **default-stable** - Same as a above, but with a stable build.

For this to work, the default push source must be configured in NuGet.config: https://docs.microsoft.com/en-us/nuget/consume-packages/configuring-nuget-behavior#nugetdefaultsconfig-settings

For example it would be possible to create additional build configurations within the csproj (Release-Stable, Prerelease), which would set the **SppStageName** property accordingly.

### **3.6 Versioning**
Local source packages are per default versioned as prerelease package and will have a postfix appended to the configured version number: **u&lt;UTC-TIME&gt;-pre**

### **3.7 Debugging support**
Local builds by default output .pdb files to the default symbol cache directory **%LocalAppData%\Temp\SymbolCache)**.
To change the symbol cache directory, set the **SppSymbolCacheDir** MSBuild property or configure in NuGet.config under "config" - add - "symbolCacheDir"

For debugging to work in Visual Studio the symbol cache path should be set up under: DEBUG - Options - Symbols - Cache symbols in this directory.  
To disable this option, define the constant: **SPP_DISABLE_COPY_LOCAL_SOURCE_PDB** or set the MSBuild property: **SppCopyLocalSourcePdbToSymbolCache** to false.

## **4. CI builds**
The difference to local builds is that CI builds are typically configuring a number of MSBuild properties on the build server

### **4.1 Staging sources with Stage Matchers**
Stage Matchers are used to determine the source for publishing packages.  
The following stages are supported: **Production**, **Integration**, **Development**.

The sources can be defined by setting the following MSBuild properties:
- **SppProduction**
- **SppIntegration**
- **SppDevelopment**

All three follow the format:
```
StageMatcherRegex=>[#StagingName][&PrereleaseVersionFormat] [ApiKey@]SourceUri[ {LatestVersionUri} ][ | [SymbolApiKey@]SymbolSourceUri].
```
**StagingName** can be used to override the default staging names.  
**PrereleaseVersionFormat** and **MetadataFormat** can be used to change how the prerelease part of the version is created:
  - **{0,Stage}** - Stage name
  - **{1,DateTime}** - DateTime.UtcNow formatted as yyyyMMdd-HHmmss
  - **{2,DateTimeValue}** - DateTime.UtcNow
  - **{3,Prefix}** - Prefix (The value of the optional Prefix group in the SourceMatcherRegex)
  - **{4,Postfix}** - Postfix (The value of the optional Postfix group in the SourceMatcherRegex)
  - **{5,Metadata}** - Metadata
  - **{6,Parameter}** - The value of the SppParameter MSBuild property (Can be used to pass in a git hash etc.)
  The following command can be used to get the short hash and send it to a GitHub Action output using git and [CommandlineBatcher](https://github.com/sundews/CommandlineBatcher) (cb).
```git rev-parse --short=10 HEAD | cb -c "|::set-output name=git_hash::{0}" --batches-stdin```

Leading and trailing dashes "-" will be trimmed.

#### **4.1.1 Stage selection**
The stage selecting is made in combination of the StageMatcherRegex and the **SppStageName** MSBuild property.  
The regexes will be evaluated in the order as listed above.

The StageMatcherRegex can be used to match the current branch (must be passed into MSBuild via build server) to decide which source to publish to and map to a staging name for the version prefix. The regex also supports two groups (Prefix and Postfix), which if found will be included in the prerelease version according to the following format:
**[&lt;Prefix&gt;-]u&lt;UTC&gt;-&lt;StagingName&gt;[-&lt;Postfix&gt;]**.

Note that using the prefix may break how NuGet clients may automatically find the latest version based on Staging name and timestamp.

#### **4.1.3 Sample set up:**
**SppStageName** = vcs branch (e.g. master, develop, feature/&lt;FEATURE-NAME&gt;, release/&lt;VERSION-NUMBER&gt;)  
**SppProduction** = master| http://nuget-server.com/production|http://nuget-server.com/production/symbols  
**SppIntegration** = release/.+| http://nuget-server.com/integration  
**SppDevelopment** = .+| http://nuget-server.com/development  

This allows the source to be selected based on the branch name in git, etc.

#### **4.1.3 Staging names**
The staging names are prepended to the prerelease version to allow differentiating the stage of a package and ensures that NuGet clients will detect the latest prerelease in the following order.

**Local** => local  
**Development** => dev  
**Integration** => ci  
**Production** => prod (Not included in actual version)

The staging name can be used to change how NuGet clients sort prereleases.  
**Example:**  All prerelease use **pre**.  
**SppIntegration** = release/.+#**pre**=>|http://nuget-server.com/integration  
**SppDevelopment** = .+#**pre**=>|http://nuget-server.com/development  

### **4.3 Suggested versioning scheme**
**GitHub flow/Git flow**
| **Build** | Branch type     | Release     | Versioning                              | Release mode              |
| --------- | --------------- | ----------- | --------------------------------------- | ------------------------- |
| CI        | main            | stable      | &lt;Major.Minor.*&gt;                   | Push to Production NuGet  |
|           | release         | integration | &lt;Major.Minor.*&gt;u&lt;UTC&gt;-ci    | Push to Integration NuGet |
|           | feature/develop | developer   | &lt;Major.Minor.*&gt;u&lt;UTC&gt;-dev   | Push to Development NuGet |
|           | PR              | -           | &lt;Major.Minor.*&gt;u&lt;UTC&gt;-dev   | -                         |
| Local     | any             | prerelease  | &lt;Major.Minor.*&gt;u&lt;UTC&gt;-local | Push to local NuGet       |

**Trunk-based development**
| **Build** | Branch type | Release     | Versioning                              | Release mode              |
| --------- | ----------- | ----------- | --------------------------------------- | ------------------------- |
| CI        | release     | stable      | &lt;Major.Minor.*&gt;                   | Push to Production NuGet  |
|           | main        | integration | &lt;Major.Minor.*&gt;u&lt;UTC&gt;-ci    | Push to Integration NuGet |
|           | feature     | developer   | &lt;Major.Minor.*&gt;u&lt;UTC&gt;-dev   | Push to Development NuGet |
|           | PR          | -           | &lt;Major.Minor.*&gt;u&lt;UTC&gt;-dev   | -                         |
| Local     | any         | prerelease  | &lt;Major.Minor.*&gt;u&lt;UTC&gt;-local | Push to local NuGet       |

Packages for the three sources above are versioned differently:  
**SppProduction** = Stable - The version number defined in the **Version** MSBuild property *.  
**SppIntegration** = Prerelease - Adds the stage name **u&lt;UTC&gt;-ci** to the configured version number *.  
**SppDevelopment** = Prerelease - Adds the stage name **u&lt;UTC&gt;-dev** to the configured version number *.

*) The patch component  depends on the **SppVersioningMode** MSBuild property

## **5. Additional MSBuild info**
### **5.1 Additional MSBuild properties**
- **SppTimeoutInSeconds** = (default: **300**) sets the publish timeout in seconds
- **SppSkipDuplicate** = (default: **false**) instructs to skip duplicate packages 
- **SppNoServiceEndpoint** = (default: **false**) instructs not to append NuGet paths to publish url.
- **SppApiKey** = (default: **null**) specifies the NuGet api key (Fallback if not specified by source matcher)
- **SppSymbolApiKey** = (default: **null**) specifies the NuGet symbols api key
- **SppVersioningMode** = specifies the mode for versioning prerelease versions:
  - **AutomaticLatestPatch** = (default) ignores the patch component of the current version and sets it to the latest matching (Major and Minor) package patch version incremented by 1. 
  - **AutomaticLatestRevision** = ignores the revision component of the current version and sets it to the latest matching (Major and Minor and Patch) package revision version incremented by 1. 
  - **IncrementPatchIfStableExistForPrerelease** = increments the patch part with 1, if the stable version already exists.
  - **AlwaysIncrementPatch** = increments the patch part with 1.
  - **NoChange** = does not change the version number.
- **SppAllowLocalSource** = (default: **true**) specifies whether local source is allowed. Usefull for CI environments to disable local source if none of the stages where matched.
- **SppPrereleasePrefix** = (default: **null**) specifies the prefix to be used for prerelease packages.
- **SppPrereleasePostfix** = (default: **null**) specifies the postfix to be used for prerelease packages.
- **SppParameter** = (default: **empty**)
- **SppPublishLogFormat** = (default: **null**) specifies a format with which packages to be pushed can be logged. Multiple formats can be separated by |.
  - **{PackageId}** - The package id.
  - **{Version}** - The normalized package version
  - **{FullVersion}** - The full package version
  - **{VersionMajor}** - The full package version
  - **{VersionMinor}** - The full package version
  - **{VersionPatch}** - The full package version
  - **{VersionRevision}** - The full package version
  - **{VersionRelease}** - The package path
  - **{Stage}** - The selected stage
  - **{VersionStage}** - The version stage
  - **{StagePromotion}** - The stage promotion (none or promoted)
  - **{PushSource}** - The selected push source
  - **{ApiKey}** - The api key
  - **{FeedSource}** - The selected feed source
  - **{SymbolsPath}** - The symbol package path
  - **{SymbolsPushSource}** - The selected symbol package source
  - **{SymbolsApiKey}** - The symbol api key
  - **{Metadata}** - The metadata
  - **{WorkingDirectory}** - The working directory
  - **{Parameter}** - The value of the SppParameter MSBuild property
  - **{DQ}** - Double quote
  - **{NL}** - Environment.NewLine
  - **{PackagePath}** - The package path

   Usefull for CI environments to extract information from the build. E.g. to set a build variable to the select push source and path for pushing packages from the CI environment only.

- **SppAppendPublishFileLogFormat** (default: **null**) similar to SppPublishLogFormat, but takes values in the format: **Format > filename.ext** and always appends. Multiple formats can be separated by |.  
  - Supports the same values as **SppPublishLogFormat**.  
  - Relative paths use the working directory  
  - The space between **Format** and **>** is ignored, to include spaces at the end, add additional ones.

- **SppLatestVersionSources** = (default: **null**) A pipe (|) separated list of sources to query to find the latest version.
- **SppAddNuGetOrgSourceToLatestVersionSources** = (default: **true**) Adds the NuGet.org to SppLatestVersionSources.
- **SppAddAllSourcesToLatestVersionSources** = (default: **true**) Adds all sources to lastest version sources.
- **SppLocalPackageStage** (default: **true**) Local builds will use the specified stage.
- **SppPrereleaseFormat** = (default: **null**) Sets the fallback prerelease format for prerelease source if not specified in the Source Matcher.
- **SppMetadata** = (default: **null**) The metadata.
- **SppMetadataFormat** = (default: **null**) The metadata format used to format the version metadata.
- **SppForceVersion** = (default: **null**) Forces the version to the specified value if not null.
- **SppDisable** = (default: **null**) Disables SPP completely.
- **SppWorkLocally** = (default: **false**) Enables package creation is set.

### **5.2 Build output**
The build also outputs MSBuild TaskItems:  
**SppPackages**
The items contains the following information:
- **ItemSpec** = The absolute package path
- **PackageSource** = is where the package was pushed to
- **Published** = indicates whether the package was published
- **IsSymbol** = indicates whether the package is a symbols package

## **6. Extensibility**
The combination of build output and disabling publication allows to override the publish functionality. By creating a MSBuild target, which runs after the **SppPublishNuGet** target and that consumes the **SppPackages** TaskItems, it is possible to create a custom way of publishing packages. This could be usefull for some CI setups where the build itself is not responsible for publishing packages, but rather to instruct the CI, which packages should be published and where.

## **7. Updating packages**
- Use the standard NuGet client

Performing package updates in large repositories can take a long time with the default NuGet client. As an alternative check out Sundew.Packaging.Tool:
- Sundew.Packaging.Tool (PackageReference only) - https://github.com/sundews/Sundew.Packaging

dotnet tool install -g Sundew.Packaging.Tool

## **8. Samples**
The projects listed at the link below use Sundew.Packaging.Publish to automate publishing packages for various stages and tag stable versions in git:  
https://github.com/sundews/builds

**GitHub flow workflow (Libraries and frameworks)**  
Also compatible with git flow for more manual control over the release process
https://github.com/sundews/Sundew.Generator/blob/main/.github/workflows/dotnet.yml

https://github.com/sundews/Sundew.CommandLine/blob/main/.github/workflows/dotnet.yml

**(Scaled) Trunk based development workflow (Applications)**
https://github.com/sundews/CommandlineBatcher/blob/main/.github/workflows/dotnet.yml

https://github.com/sundews/Sundew.Packaging/blob/main/.github/workflows/dotnet.yml

# **Sundew.Packaging.Tool**

## **1. Description**
* Alternative NuGet client for bulk updating NuGet packages in csproj, fsproj and vbproj projects (PackageReference only).
* Await NuGet package being published.
* Prune NuGet packages from a local source.

## **2. Install**
Sundew.Packaging.Tool uses multi targetted tool versioning, which means the versioning scheme follows the pattern {SupportedRuntime}.{Major}.{Minor}.{Patch}.
This means that the install can be fixed to a version of the tool running on a specific runtime version. .NET 3.1 (3), 6 and 7 are supported.
It is available on NuGet at:  https://www.nuget.org/packages/Sundew.Packaging.Tool
dotnet tool install Sundew.Packaging.Tool -g --version 7.8.*

## **3. Usage**

```
Help
 Verbs:
   stage-build/sb                   Stages a build
     -pf  | --project-file          | The project file                                                                                          | Required
     -s   | --stage                 | The stage used to match against the production, integration and development sources                       | Required
     Stage selection                                                                                                                            | Required
      -p  | --production            | The production stage used to determine the stable version.                                                | Default: [none]
                                      Format: Stage Regex =>[ #StagingName][ &PrereleaseVersionFormat] [ApiKey@]SourceUri[ {LatestVersionUri} ]
                                      [ | [SymbolApiKey@]SymbolSourceUri][|[|PropertyName=PropertyValue]*]
      -i  | --integration           | The integration stage used to determine the prerelease version.                                           | Default: [none]
      -d  | --development           | The development stage  used to determine the prerelease version.                                          | Default: [none]
      -n  | --no-stage              | The fallback stage and properties if no stage is matched.                                                 | Default: [none]
                                      [#StagingName|][PropertyName=PropertyValue]*
     -spi | --stage-promotion-input | The input used to determine if build stage should be promoted                                             | Default: [none]
                                      Use <filename to match against the content of a file.
     -spr | --stage-promotion-regex | The regex to match against the stage-promotion-input.                                                     | Default: [none]
     -wd  | --directory             | The working directory or file used to determine the base version.                                         | Default: [none]
     -c   | --configuration         | The configuration used to evaluate the project file.                                                      | Default: [none]
     -pp  | --prerelease-prefix     | The prerelease prefix.                                                                                    | Default: [none]
     -ps  | --prerelease-postfix    | The prerelease postfix.                                                                                   | Default: [none]
          | --prerelease-format     | The prerelease format.                                                                                    | Default: [none]
     -m   | --metadata              | The version metadata.                                                                                     | Default: [none]
     -vm  | --versioning-mode       | The versioning mode: [a]utomatic-latest-patch, [automatic]-latest-revision,                               | Default: automatic-latest-patch
                                      [i]ncrement-patch-if-stable-exist-for-prerelease, [always]-increment-patch, [n]o-change
     -vf  | --version-format        | The version format                                                                                        | Default: [none]
     -fv  | --force-version         | Forces the version to the specified value                                                                 | Default: [none]
     -fe  | --file-encoding         | The name of the encoding e.g. utf-8, utf-16/unicode.                                                      | Default: [none]
     -o   | --output-formats        | A list of formats that will be logged to stdout.                                                          | Default: [none]
                                      Use redirection format (>[filename]|output-format) to output to a file.
     -of  | --output-file           | The file path to be used for output formats that specifies empty redirection >|                           | Default: [none]
   push                             Pushes the specified package(s) to a source
     -s   | --source                | The source used to push packages.                                                                         | Required
     -k   | --api-key               | The api key to be used for the push.                                                                      | Required
     -ss  | --symbol-source         | The source used to push symbol packages.                                                                  | Default: [none]
     -sk  | --symbol-api-key        | The symbols api key used to push symbols.                                                                 | Default: [none]
     -t   | --timeout               | Timeout for pushing to a source (seconds).                                                                | Default: 300
     -r   | --retries               | The number of retries to push the package in case of a failure.                                           | Default: 0
     -n   | --no-symbols            | If set no symbols will be pushed.
     -sd  | --skip-duplicate        | If a package already exists, skip it.
     <packages>                     | The packages to push (supports wildcards *).                                                              | Required
   update/u                         Update package references
     -id  | --package-ids           | The package(s) to update. (* Wildcards supported)                                                         | Default: *
                                      Format: Id[.Version] or "Id[ Version]" (Pinning version is optional)
     -p   | --projects              | The project(s) to update (* Wildcards supported)                                                          | Default: *
     -s   | --source                | The source or source name to search for packages ("All" supported)                                        | Default: NuGet.config: All
          | --version               | The NuGet package version (* Wildcards supported).                                                        | Default: Latest version
     -d   | --root-directory        | The directory to search for projects                                                                      | Default: Current directory
     -pr  | --prerelease            | Allow updating to latest prerelease version
     -v   | --verbose               | Verbose
     -l   | --local                 | Forces the source to "Local-SPP"
     -sr  | --skip-restore          | Skips a dotnet restore command after package update.
   await/a                          Awaits the specified package to be published
     -s   | --source                | The source or source name to await publish                                                                | Default: NuGet.config: defaultPushSource
     -d   | --root-directory        | The directory to search for projects                                                                      | Default: Current directory
     -t   | --timeout               | The wait timeout in seconds                                                                               | Default: 300
     -v   | --verbose               | Verbose
     <package-id>                   | Specifies the package id and optionally the version                                                       | Required
                                      Format: <PackageId>[.<Version>].
                                      If the version is not provided, it must be specified by the version value
     <version>                      | Specifies the NuGet Package version                                                                       | Default: [none]
   prune/p                          Prunes the matching packages for a local source
     all                            Prune the specified local source for all packages
       -p | --package-ids           | The packages to prune (* Wildcards supported)                                                             | Default: *
       -s | --source                | Local source or source name to search for packages                                                        | Default: Local-SPP
       -v | --verbose               | Verbose
   delete/d                         Delete files
     -d   | --root-directory        | The directory to search for projects                                                                      | Default: Current directory
     -r   | --recursive             | Specifies whether to recurse into subdirectories.
     -v   | --verbose               | Verbose
     <files>                        | The files to be deleted                                                                                   | Required
```

## **3. Examples**
Open Package Manager Console in Visual Studio or a similar command line.
Stage Build
1. https://github.com/sundews/Sundew.Packaging/blob/main/.github/workflows/dotnet.yml  

Update
1. ```spt update``` - Update all packages in all projects to the latest stable version.
2. ```spt u -id Serilog*``` - Updates all Serilog packages to the latest stable version for all projects (That reference Serilog)
3. ```spt u -id TransparentMoq -pr -l``` - Updates TransparentMoq to the latest prerelease from the "Local-Sundew" source (Useful together with Sundew.Packaging.Publish for local development).

Await
1. ```spt await TransparentMoq.4.16.2``` - Awaits TransparentMoq.4.0.0 to be published to the default push source.
2. ```spt a -s MySource TransparentMoq.4.16.2``` - Awaits TransparentMoq.4.0.0 to be published to the source named MySource.
3. ```spt a -t 60 TransparentMoq.4.16.2``` - Awaits TransparentMoq.4.0.0 to be published to the default push source, but times out after one minute.
4. ```spt``` used with Sundew.Packaging.Publish to ensure that another stable build does not run until packages are published https://github.com/sundews/Sundew.Generator/blob/main/.github/workflows/dotnet.yml

Prune
1. ```spt prune all``` - Prunes the all packages in the default local source.
2. ```spt p all -p Serilog* -s MySource``` - Prunes all packages starting with Serilog from the source named MySource.
3. ```spt p all -p TransparentMoq``` - Prunes all TransparentMoq packages from the default local source.

# **PaketLocalUpdate**

## **1. Description**
If you are using paket as a NuGet Client, working with local packages are not supported for SDK-style projects.
PaketLocalUpdate (plu) works around this limitation allowing you to work with local packages through the following steps:

1. Temporarily updates your paket.dependencies file:
2. Adds the specified local source (Default the NuGet configured Local-SPP)
3. Marks the specified packages as prerelease
4. Runs a paket update
5. Reverts paket.dependencies file

Attention! Do not commit your paket.lock file with local dependencies!

## **2. Install**
PaketLocalUpdate uses multi targetted tool versioning, which means the versioning scheme follows the pattern {SupportedRuntime}.{Major}.{Minor}.{Patch}.
This means that the install can be fixed to a version of the tool running on a specific runtime version. .NET 3.1, 6 and 7 are supported.
It is available on NuGet at: https://www.nuget.org/packages/PaketLocalUpdate
dotnet tool install PaketLocalUpdate -g --version 7.4.*

## **3. Usage**
```
Help
 Arguments:      Update and restore local packages without paket.local files
  -s | --source  | The local source or its name to be temporarily added to search for packages | Default: Local-SPP
  -g | --group   | The group name                                                              | Default: Main
  -V | --version | The version constraint                                                      | Default: [none]
  -f | --filter  | Specifies whether the package id should be treated as a regex
  -v | --verbose | Enable verbose logging
  <package-id>   | Package id or pattern                                                       | Default: *
```
# Sundew.Packaging.Publish
Previously: Sundew.Build.Publish

## **1. Description**
Sundew.Packaging.Publish is an automatic package publisher that can work standalone locally or integrated as a part of a CI/CD pipeline.
- Automation for multi-repos
- Publish prerelease package to local NuGet feed
- Publish to official NuGet feed (or default push source)
- Customizable publish for CI based on development, integration, production stages
- Automated patch and prerelease versioning
- Local debug support

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
Once installed, Sundew.Packaging.Publish will start producing local builds and output them to the default local source: **(%LocalAppData%\Sundew.Packaging.Publish\packages)**.

### **3.2 Local source from NuGet.config**
NuGet.config provides a hierarical way of configuring NuGet from the local solution folder to the machinewide settings.  
During build, the used local source will be added to the solution specific NuGet.config with the name: **Local-Sundew** (If the name does not already exist).  
This allows the default local source to be overwritten by adding a package source named **Local (Sundew)**.  
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

For example it would be possible to create additional build configurations within the csproj (Release-Stable, Prerelease), which would set the **SppSourceName** property accordingly.

### **3.6 Versioning**
Local source packages are per default versioned as prerelease package and will have a postfix appended to the configured version number: **u&lt;UTC-TIMESTAMP&gt;-pre**

### **3.7 Debugging support**
Local builds by default output .pdb files to the default symbol cache directory **%LocalAppData%\Temp\SymbolCache)**.
To change the symbol cache directory, set the **SppSymbolCacheDir** MSBuild property or configure in NuGet.config under "config" - add - "symbolCacheDir"

For debugging to work in Visual Studio the symbol cache path should be set up under: DEBUG - Options - Symbols - Cache symbols in this directory.  
To disable this option, define the constant: **SPP_DISABLE_COPY_LOCAL_SOURCE_PDB** or set the MSBuild property: **SppCopyLocalSourcePdbToSymbolCache** to false.

## **4. CI builds**
The difference to local builds is that CI builds are typically configuring a number of MSBuild properties on the build server

### **4.1 Staging sources with Source Matchers**
Source Matchers are used to determine the source for publishing packages.  
The following stages are supported: **Production**, **Integration**, **Development**.

The sources can be defined by setting the following MSBuild properties:
- **SppProductionSource**
- **SppIntegrationSource**
- **SppDevelopmentSource**

All three follow the format:
**SourceMatcherRegex\[ #StagingName\]\[ $PrereleaseVersionFormat\]=> \[ApiKey@\]SourceUri\[ \{LatestVersionUri\} \]\[ | [SymbolApiKey@\]SymbolSourceUri\]**.

**StagingName** can be used to override the default staging names.  
**PrereleaseVersionFormat** can be used to change how the prerelease part of the version is created:
  - **{0}** - Staging name
  - **{1}** - DateTime.UtcNow formatted as yyyyMMdd-HHmmss
  - **{2}** - DateTime.UtcNow
  - **{3}** - Prefix (The value of the optional Prefix group in the SourceMatcherRegex)
  - **{4}** - Postfix (The value of the optional Postfix group in the SourceMatcherRegex)
  - **{5}** - The value of the SppParameter MSBuild property (Can be used to pass in a git hash etc.)

Leading and trailing dashes "-" will be trimmed.

#### **4.1.1 Source selection**
The source selecting is made in combination of the SourceMatcherRegex and the **SppSourceName** MSBuild property.  
The regexes will be evaluated in the order as listed above.

The SourceMatcherRegex can be used to match the current branch (must be passed into MSBuild via build server) to decide which source to publish to and map to a staging name for the version prefix. The regex also supports two groups (Prefix and Postfix), which if found will be included in the prerelease version according to the following format:
**[&lt;Prefix&gt;-]u&lt;UTC-TIMESTAMP&gt;-&lt;StagingName&gt;[-&lt;Postfix&gt;]**.

Note that using the prefix may break how NuGet clients may automatically find the latest version based on Staging name and timestamp.

#### **4.1.3 Sample set up:**
**SppSourceName** = vcs branch (e.g. master, develop, feature/&lt;FEATURE-NAME&gt;, release/&lt;VERSION-NUMBER&gt;)  
**SppProductionSource** = master|http://nuget-server.com/production|http://nuget-server.com/production/symbols  
**SppIntegrationSource** = release/.+|http://nuget-server.com/integration  
**SppDevelopmentSource** = .+|http://nuget-server.com/development  

This allows the source to be selected based on the branch name in git, etc.

#### **4.1.3 Staging names**
The staging names are prepended to the prerelease version to allow differentiating the stage of a package and ensures that NuGet clients will detect the latest prerelease in the following order.

**Local** => pre  
**Development** => dev  
**Integration** => ci 
**Production** => prod (Not included in actual version)

The staging name can be used to change how NuGet clients sort prereleases.  
**Example:**  All prerelease use **pre**.  
**SppIntegrationSource** = release/.+**=>pre**|http://nuget-server.com/integration  
**SppDevelopmentSource** = .+**=>pre**|http://nuget-server.com/development  

### **4.3 Suggested versioning scheme**
**GitHub flow/Git flow**
| **Build** | Branch type     | Release     | Versioning                                       | Release mode              |
| --------- | --------------- | ----------- | ------------------------------------------------ | ------------------------- |
| CI        | main            | stable      | &lt;Major.Minor.*&gt;                            | Push to Production NuGet  |
|           | release         | integration | &lt;Major.Minor.*&gt;u&lt;UTC-TIMESTAMP&gt;-ci   | Push to Integration NuGet |
|           | feature/develop | developer   | &lt;Major.Minor.*&gt;u&lt;UTC-TIMESTAMP&gt;-dev  | Push to Development NuGet |
|           | PR              | -           | &lt;Major.Minor.*&gt;u&lt;UTC-TIMESTAMP&gt;-dev  | -                         |
| Local     | any             | prerelease  | &lt;Major.Minor.*&gt;u&lt;UTC-TIMESTAMP&gt;-pre  | Push to local NuGet       |

**Trunk based development**
| **Build** | Branch type | Release     | Versioning                                        | Release mode              |
| --------- | ----------- | ----------- | ------------------------------------------------- | ------------------------- |
| CI        | release     | stable      | &lt;Major.Minor.*&gt;                             | Push to Production NuGet  |
|           | main        | integration | &lt;Major.Minor.*&gt;u&lt;UTC-TIMESTAMP&gt;-ci    | Push to Integration NuGet |
|           | feature     | developer   | &lt;Major.Minor.*&gt;u&lt;UTC-TIMESTAMP&gt;-dev   | Push to Development NuGet |
|           | PR          | -           | &lt;Major.Minor.*&gt;u&lt;UTC-TIMESTAMP&gt;-dev   | -                         |
| Local     | any         | prerelease  | &lt;Major.Minor.*&gt;u&lt;UTC-TIMESTAMP&gt;-pre   | Push to local NuGet       |

Packages for the three sources above are versioned differently:  
**SppProductionSource** = Stable - The version number defined in the **Version** MSBuild property *.  
**SppIntegrationSource** = Prerelease - Adds the stage name **u&lt;UTC-TIMESTAMP&gt;-ci** to the configured version number *.  
**SppDevelopmentSource** = Prerelease - Adds the stage name **u&lt;UTC-TIMESTAMP&gt;-dev** to the configured version number *.

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
- **SppParameter** = (default: **empty**)
- **SppPublishLogFormat** = (default: **null**) specifies a format with which packages to be pushed can be logged. Multiple formats can be separated by |.
  - **{0}** - The package id.
  - **{1}** - The resulting package version
  - **{2}** - The package path
  - **{3}** - The selected stage
  - **{4}** - The selected push source
  - **{5}** - The api key
  - **{6}** - The selected feed source
  - **{7}** - The symbol package path
  - **{8}** - The selected symbol package source
  - **{9}** - The symbol api key
  - **{10}** - The value of the SppParameter MSBuild property
  - **{11}** - Double quotes

   Usefull for CI environments to extract information from the build. E.g. to set a build variable to the select push source and path for pushing packages from the CI environment only.

- **SppAppendPublishFileLogFormat** (default: **null**) similar to SppPublishLogFormat, but takes values in the format: **Format > filename.ext** and always appends. Multiple formats can be separated by |.  
  - Supports the same values as **SppPublishLogFormat**.  
  - Relative paths use the working directory  
  - The space between **Format** and **>** is ignored, to include spaces at the end, add additional ones.

- **SppLatestVersionSources** = (default: **null**) A pipe (|) separated list of sources to query to find the latest version.
- **SppAddDefaultPushSourceToLatestVersionSources** = (default: **true**) Adds the default push source to SppLatestVersionSources.
- **SppPrereleaseFormat** = (default: **null**) Sets the fallback prerelease format for prerelease source if not specified in the Source Matcher.

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
- Sundew.Packaging.Tool (PackageReference only) - https://github.com/hugener/Sundew.Packaging.Tool

dotnet tool install -g Sundew.Packaging.Tool

## **8. Samples**
The projects listed at the link below use Sundew.Packaging.Publish to automate publishing packages for various stages and tag stable versions in git:  
https://github.com/hugener/builds

- [GitHub actions sample](https://github.com/hugener/Sundew.TextView.ApplicationFramework/blob/master/.github/workflows/dotnet.yml)  
- [Azure Pipeline sample](https://github.com/hugener/Sundew.Generator/blob/main/azure-pipelines.yml)

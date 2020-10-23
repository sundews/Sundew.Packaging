# Sundew.Build.Publish

## **1. Description**
Sundew.Build.Publish is an automatic package publisher that can work standalone locally or integrated as a part of a CI/CD pipeline.
- Automation for multi-repos
- Publish prerelease package to local NuGet feed
- Publish to official NuGet feed (or default push source)
- Customizable publish for CI based on development, integration, production stages
- Automated versioning of prereleases
- Local debug support

## **2. Setup**
Sundew.Build.Publish is available on NuGet at: https://www.nuget.org/packages/Sundew.Build.Publish

### **2.1 Disabling Sundew.Build.Publish**
There are two ways to disable the automatic publishing.
- Define constant **DISABLE_SBP** - Disables Sundew.Build.Publish entirely
- Set MSBuild property **SbpEnablePublish** to false - Disables publish
- Define constant **SBP_DISABLE_PUBLISH** - Same as above

## **3. Local builds**
Local builds are NuGet packages that are deployed to the local machine. This allows the packages to be consumed instantenously by other projects.

### **3.1 Setting up local builds**
Once installed, Sundew.Build.Publish will start producing local builds and output them to the default local source: **(%LocalAppData%\Sundew.Build.Publish\packages)**.

### **3.2 Local source from NuGet.config**
NuGet.config provides a hierarical way of configuring NuGet from the local solution folder to the machinewide settings.<br>
During build, the used local source will be added to the solution specific NuGet.config with the name: **Local (Sundew)** (If the name does not already exist).<br>
This allows the default local source to be overwritten by adding a package source named **Local (Sundew)**.<br/>
Use the NuGet commandline to modify the settings: https://docs.microsoft.com/en-us/nuget/consume-packages/configuring-nuget-behavior

### **3.3 Local source from project**
The local source can also be overwritten by settings the **SbpLocalSource** MSBuild property.

### **3.4 Stable local builds**
Local build can be created with a stable version number by setting the  **SbpSourceName** MSBuild property to: **local-stable**

### **3.5 Publishing local builds**
Local builds can also be published by setting the **SbpSourceName** MSBuild property.<br/>
Valid values are:
- **default** - Creates a prerelease build and publishes it to the default push source.
- **default-stable** - Same as a above, but with a stable build.

For this to work, the default push source must be configured in NuGet.config: https://docs.microsoft.com/da-dk/nuget/consume-packages/configuring-nuget-behavior#nugetdefaultsconfig-settings

For example it would be possible to create additional build configurations within the csproj (Release-Stable, Prerelease), which would set the **SbpSourceName** property accordingly.

### **3.6 Versioning**
Local source packages are per default versioned as prerelease package and will have a postfix appended to the configured version number: **pre-u&lt;UTC-TIMESTAMP&gt;**

### **3.7 Debugging support**
Local builds by default output .pdb files to the default symbol cache directory **%LocalAppData%\Temp\SymbolCache)**.
To change the symbol cache directory, set the **SbpSymbolCacheDir** MSBuild property or configure in NuGet.config under "config" - add - "symbolCacheDir"

For debugging to work in Visual Studio the symbol cache path should be set up under: DEBUG - Options - Symbols - Cache symbols in this directory.<br/>
To disable this option, define the constant: **SBP_DISABLE_COPY_LOCAL_SOURCE_PDB** or set the MSBuild property: **SbpCopyLocalSourcePdbToSymbolCache** to false.

## **4. CI builds**
The difference to local builds is that CI builds are typically configuring a number of MSBuild properties on the build server

### **4.1 Staging sources**
Staging sources are used to determine the source for publishing packages.<br>
The following stages are supported: **Production**, **Integration**, **Development**.

The sources can be defined by settings the following MSBuild properties:
- **SbpProductionSource**
- **SbpIntegrationSource**
- **SbpDevelopmentSource**

All three follow the format:
**SourceMatcherRegex|SourceUri[|SymbolSourceUri]**.<br>
Escape | (pipes) with another pipe, if needed in the regex.

### **4.2 Source selection**
The source selecting is made in combination of the SourceMatcherRegex and the **SbpSourceName** MSBuild property.<br>
The regexes will be evaluated in the order as listed above.

#### **Sample set up:**
**SbpSourceName** = vcs branch (e.g. master, dev, feature/&lt;FEATURE-NAME&gt;, release/&lt;VERSION-NUMBER&gt;)<br>
**SbpProductionSource** = master|http://nuget-server.com/production|http://nuget-server.com/production/symbols<br>
**SbpIntegrationSource** = release/.+|http://nuget-server.com/integration<br>
**SbpDevelopmentSource** = .+|http://nuget-server.com/development<br>

This allows the source to be selected based on the branch name in git, etc.

### **4.3 Versioning**
Packages for the three sources above are versioned differently:<br>
**SbpProductionSource** = Stable - The version number defined in csproj<br>
**SbpIntegrationSource** = Prerelease - Adds the postfix **int-u&lt;UTC-TIMESTAMP&gt;** to the configured version number.<br>
**SbpDevelopmentSource** = Prerelease - Adds the postfix **dev-u&lt;UTC-TIMESTAMP&gt;** to the configured version number.

## **5. Additional MSBuild info**
### **5.1 Additional MSBuild properties**
- **SbpTimeoutInSeconds** = sets the publish timeout in seconds (Default: 300)
- **SbpSkipDuplicate** = instructs to skip duplicate packages (Default: false)
- **SbpNoServiceEndpoint** = instructs not to append NuGet paths to publish url. (Default: false)
- **SbpApiKey** = specifies the NuGet api key
- **SbpSymbolApiKey** = specifies the NuGet symbols api key
- **SbpPrereleaseVersioningMode** = specifies the mode for versioning prerelease versions:
  - **Automatic** = (default) increments the patch part with 1, if the stable version already exists.
  - **IncrementPatch** = increments the patch part with 1.
  - **NoChange** = does not change the version number.
- **SbpAllowLocalSource** = (default: true) specifies whether local source is allowed. Usefull for CI environments to disable local source if none of the stages where matched.
- **SbpPublishLogFormat** = (default: null) specifies a format with which packages to be pushed can be logged. Multiple log formats can be separated by |.
  - **{0}** - The package id.
  - **{1}** - The resulting package version
  - **{2}** - The selected package source
  - **{3}** - The package path
  
   Usefull for CI environments to extract information from the build. E.g. to set a build variable to the package source and path for pushing packages from the CI environment only.

### **5.2 Build output**
The build also outputs MSBuild TaskItems:<br> 
**SbpPackages**
The items contains the following information:
- **ItemSpec** = The absolute package path
- **PackageSource** = is where the package was pushed to
- **Published** = indicates whether the package was published
- **IsSymbol** = indicates whether the package is a symbols package

## **6. Extensibility**
The combination of build output and disabling publication allows to override the publish functionality. By creating a MSBuild target, which runs after the **SbpPublishNuGet** target and that consumes the **SbpPackages** TaskItems, it is possible to create a custom way of publishing packages. This could be usefull for some CI setups where the build itself is not responsible for publishing packages, but rather to instruct the CI, which packages should be published and where. 
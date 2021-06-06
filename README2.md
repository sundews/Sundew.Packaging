# Sundew.Packaging.Tool

## **1. Description**
* Alternative NuGet client for bulk updating NuGet packages in csproj, fsproj and vbproj projects (PackageReference only).
* Await NuGet package being published.
* Prune NuGet packages from a local source.

## **2. Install**
dotnet tool install -g Sundew.Packaging.Tool

## **3. Usage**

```
Help
 Verbs:
   update/u                  Update package references
     -id  | --package-ids    | The package(s) to update. (* Wildcards supported)                         | Default: *
                               Format: Id[.Version] or "Id[ Version]" (Pinning version is optional)
     -p   | --projects       | The project(s) to update (* Wildcards supported)                          | Default: *
     -s   | --source         | The source or source name to search for packages ("All" supported)        | Default: NuGet.config: defaultPushSource
          | --version        | The NuGet package version (* Wildcards supported).                        | Default: Latest version
     -d   | --root-directory | The directory to search for projects                                      | Default: Current directory
     -pr  | --prerelease     | Allow updating to latest prerelease version
     -v   | --verbose        | Verbose
     -l   | --local          | Forces the source to "Local-Sundew"
     -sr  | --skip-restore   | Skips a dotnet restore command after package update.
   await/a                   Awaits the specified package to be published
     -s   | --source         | The source or source name to await publish                                | Default: NuGet.config: defaultPushSource
     -d   | --root-directory | The directory to search for projects                                      | Default: Current directory
     -t   | --timeout        | The wait timeout in seconds                                               | Default: 300
     -v   | --verbose        | Verbose
     <package-id>            | Specifies the package id and optionally the version                       | Required
                               Format: <PackageId>[.<Version>].
                               If the version is not provided, it must be specified by the version value
     <version>               | Specifies the NuGet Package version                                       | Default: [none]
   prune/p                   Prunes the matching packages for a local source
     all                     Prune the specified local source for all packages
       -p | --package-ids    | The packages to prune (* Wildcards supported)                             | Default: *
       -s | --source         | Local source or source name to search for packages                        | Default: Local-Sundew
       -v | --verbose        | Verbose
   delete/d                  Delete files
     -d   | --root-directory | The directory to search for projects                                      | Default: Current directory
     -r   | --recursive      | Specifies whether to recurse into subdirectories.
     -v   | --verbose        | Verbose
     <files>                 | The files to be deleted                                                   | Required
```

## **3. Examples**
Open Package Manager Console in Visual Studio or a similar command line.

Update
1. ```spt update``` - Update all packages in all projects to the latest stable version.
2. ```spt u -id Serilog*``` - Updates all Serilog packages to the latest stable version for all projects (That reference Serilog)
3. ```spt u -id TransparentMoq -pr -l``` - Updates TransparentMoq to the latest prerelease from the "Local-Sundew" source (Useful together with Sundew.Packaging.Publish for local development).

Await
1. ```spt await TransparentMoq.4.16.2``` - Awaits TransparentMoq.4.0.0 to be published to the default push source.
2. ```spt a -s MySource TransparentMoq.4.16.2``` - Awaits TransparentMoq.4.0.0 to be published to the source named MySource.
3. ```spt a -t 60 TransparentMoq.4.16.2``` - Awaits TransparentMoq.4.0.0 to be published to the default push source, but times out after one minute.
4. ```spt``` used with Sundew.Packaging.Publish to ensure that another stable build does not run until packages are published https://github.com/hugener/Sundew.Generator/blob/main/.github/workflows/dotnet.yml

Prune
1. ```spt prune all``` - Prunes the all packages in the default local source.
2. ```spt p all -p Serilog* -s MySource``` - Prunes all packages starting with Serilog from the source named MySource.
3. ```spt p all -p TransparentMoq``` - Prunes all TransparentMoq packages from the default local source.

Sundew.Packaging.Publish is available on NuGet at: https://www.nuget.org/packages/Sundew.Packaging.Publish
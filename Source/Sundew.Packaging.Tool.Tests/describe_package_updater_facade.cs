// --------------------------------------------------------------------------------------------------------------------
// <copyright file="describe_package_updater_facade.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#pragma warning disable 8602
namespace Sundew.Packaging.Tool.Tests
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Threading;
    using global::NuGet.Versioning;
    using Moq;
    using Sundew.Base.Collections;
    using Sundew.Base.Text;
    using Sundew.Packaging.Tool.Diagnostics;
    using Sundew.Packaging.Tool.Update;
    using Sundew.Packaging.Tool.Update.MsBuild;
    using Sundew.Packaging.Tool.Update.MsBuild.NuGet;

    public class describe_package_updater_facade : nspec
    {
        void when_updating_packages()
        {
            this.beforeEach = () =>
             {
                 this.fileSystem = New.Mock<IFileSystem>().SetDefaultValue(DefaultValue.Mock);
                 this.nuGetPackageVersionFetcher = New.Mock<INuGetPackageVersionFetcher>();
                 this.packageVersionSelectorReporter = New.Mock<IPackageVersionSelectorReporter>();
                 this.processRunner = New.Mock<IProcessRunner>();
                 this.packageUpdaterFacade = new PackageUpdaterFacade(
                     this.fileSystem,
                     this.nuGetPackageVersionFetcher,
                     this.processRunner,
                     New.Mock<IPackageUpdaterFacadeReporter>(),
                     New.Mock<IPackageVersionUpdaterReporter>(),
                     this.packageVersionSelectorReporter,
                     New.Mock<IPackageRestorerReporter>());

                 TestData.GetPackages().ForEach(x =>
                 {
                     this.nuGetPackageVersionFetcher.Setup(n => n.GetAllVersionsAsync(
                             It.IsAny<string>(),
                             It.IsAny<string>(),
                             x.Id))
                         .ReturnsAsync(x.Versions);
                 });
             };

            this.actAsync = () => this.packageUpdaterFacade?.UpdatePackagesInProjectsAsync(this.arguments!);

            const string separator = ", ";
            this.context[$"given projects: {TestData.GetProjects().Select(x => x.Path).JoinToString(separator)}"] = () =>
            {
                this.beforeEach = () =>
                {
                    this.fileSystem!.Directory.Setup(x => x.GetCurrentDirectory()).Returns(TestData.RootDirectory);
                    this.fileSystem.Directory.Setup(x =>
                        x.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), SearchOption.AllDirectories)).Returns(
                        TestData.GetProjects().Select(x => x.Path));
                    TestData.GetProjects().ForEach(x =>
                    {
                        this.fileSystem.File.Setup(f => f.ReadAllTextAsync(x.Path, CancellationToken.None)).ReturnsAsync(x.Source);
                    });
                };

                this.context["and arguments: package id: Sundew*"] = () =>
                {
                    this.beforeEach = () => this.arguments = new UpdateVerb(new List<PackageId> { new("Sundew*") }, new List<string>());

                    this.context["and arguments: projects: Sundew*"] = () =>
                    {
                        this.beforeEach = () => this.arguments = new UpdateVerb(this.arguments.PackageIds.ToList(), new List<string> { "Sundew*" });

                        this.context["and do not allow update to prerelease"] = () =>
                        {
                            TestData.SundewCommandLineProject.Assert(x => this.it[$@"should write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, x.ExpectedNonPrereleaseUpdatedSource, CancellationToken.None), Times.Once));

                            TestData.SundewBuildPublishProject.Assert(x => this.it[$@"should not write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, It.IsAny<string>(), CancellationToken.None), Times.Never));

                            TestData.TransparentMoqProject.Assert(x => this.it[$@"should not write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, It.IsAny<string>(), CancellationToken.None), Times.Never));

                            TestData.SundewBasePackageUpdateForSundewCommandLine.Assert(x =>
                                this.it[$"should update package: {x.Id} from {x.NuGetVersion} to {x.UpdatedNuGetVersion}"] =
                                    () => this.packageVersionSelectorReporter.Verify(r => r.PackageUpdateSelected(x.Id, x.NuGetVersion, x.UpdatedNuGetVersion), Times.Once()));

                            TestData.SundewBuildPublishPackageUpdateForSundewCommandLine.Assert(x =>
                                this.it[$"should update package: {x.Id} from {x.NuGetVersion} to {x.UpdatedNuGetVersion}"] =
                                    () => this.packageVersionSelectorReporter.Verify(r => r.PackageUpdateSelected(x.Id, x.NuGetVersion, x.UpdatedNuGetVersion), Times.Once()));

                            this.it["should not update any other packages"] = () =>
                                this.packageVersionSelectorReporter.Verify(
                                    r => r.PackageUpdateSelected(
                                        It.Is<string>(x =>
                                            x != TestData.SundewBasePackage.Id &&
                                            x != TestData.SundewBuildPublishPackage.Id),
                                        It.IsAny<NuGetVersion>(),
                                        It.IsAny<NuGetVersion>()),
                                    Times.Never);

                            this.it["should run restore"] = () => this.processRunner.Verify(x => x.Run(It.IsAny<ProcessStartInfo>()), Times.Once);
                        };

                        this.context["and does allow update to prerelease"] = () =>
                        {
                            this.beforeEach = () => this.arguments = new UpdateVerb(this.arguments.PackageIds.ToList(), this.arguments.Projects.ToList(), allowPrerelease: true);

                            TestData.SundewBuildPublishProject.Assert(x => this.it[$@"should write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, x.ExpectedPrereleaseUpdatedSource, CancellationToken.None), Times.Once));

                            TestData.SundewCommandLineProject.Assert(x => this.it[$@"should write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, x.ExpectedPrereleaseUpdatedSource, CancellationToken.None), Times.Once));

                            TestData.TransparentMoqProject.Assert(x => this.it[$@"should not write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, It.IsAny<string>(), CancellationToken.None), Times.Never));

                            TestData.SundewBasePrereleasePackageUpdateForSundewCommandLine.Assert(x =>
                                this.it[$"should update package: {x.Id} from {x.NuGetVersion} to {x.UpdatedNuGetVersion}"] =
                                    () => this.packageVersionSelectorReporter.Verify(r => r.PackageUpdateSelected(x.Id, x.NuGetVersion, x.UpdatedNuGetVersion), Times.Once()));

                            TestData.SundewBuildPublishPackageUpdateForSundewCommandLine.Assert(x =>
                                this.it[$"should update package: {x.Id} from {x.NuGetVersion} to {x.UpdatedNuGetVersion}"] =
                                    () => this.packageVersionSelectorReporter.Verify(r => r.PackageUpdateSelected(x.Id, x.NuGetVersion, x.UpdatedNuGetVersion), Times.Once()));

                            this.it["should not update any other packages"] = () =>
                                this.packageVersionSelectorReporter.Verify(
                                    r => r.PackageUpdateSelected(
                                        It.Is<string>(x =>
                                            x != TestData.SundewBasePackage.Id &&
                                            x != TestData.SundewBuildPublishPackage.Id),
                                        It.IsAny<NuGetVersion>(),
                                        It.IsAny<NuGetVersion>()),
                                    Times.Never);

                            this.it["should run restore"] = () => this.processRunner.Verify(x => x.Run(It.IsAny<ProcessStartInfo>()), Times.Once);
                        };

                        this.context["and pins Sundew.Base version to 6.0.0"] = () =>
                        {
                            this.beforeEach = () => this.arguments = new UpdateVerb(new List<PackageId> { new("Sundew.Base", "6.0.0") }, this.arguments.Projects.ToList());

                            TestData.SundewCommandLineProject.Assert(x => this.it[$@"should write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(
                                    f => f.WriteAllTextAsync(x.Path, TestData.SundewCommandLineData.PinnedSundewBaseUpdatedSource, CancellationToken.None),
                                    Times.Once));

                            TestData.TransparentMoqProject.Assert(x => this.it[$@"should not write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(
                                        f => f.WriteAllTextAsync(x.Path, It.IsAny<string>(), CancellationToken.None),
                                        Times.Never));

                            TestData.SundewBuildPublishProject.Assert(x => this.it[$@"should not write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(
                                        f => f.WriteAllTextAsync(x.Path, It.IsAny<string>(), CancellationToken.None),
                                        Times.Never));

                            TestData.SundewBasePackageUpdateForSundewCommandLine.Assert(x =>
                                this.it[$"should update package: {x.Id} from {x.NuGetVersion} to {x.UpdatedNuGetVersion}"] =
                                    () => this.packageVersionSelectorReporter.Verify(
                                            r => r.PackageUpdateSelected(x.Id, x.NuGetVersion, x.UpdatedNuGetVersion),
                                            Times.Once()));

                            this.it["should not update any other packages"] = () =>
                                this.packageVersionSelectorReporter.Verify(
                                    r => r.PackageUpdateSelected(
                                        It.Is<string>(x => x != TestData.SundewBasePackage.Id),
                                        It.IsAny<NuGetVersion>(),
                                        It.IsAny<NuGetVersion>()),
                                    Times.Never);

                            this.it["should run restore"] = () => this.processRunner.Verify(x => x.Run(It.IsAny<ProcessStartInfo>()), Times.Once);

                            this.context["and skip-restore is set"] = () =>
                            {
                                this.beforeEach = () => this.arguments = new UpdateVerb(this.arguments.PackageIds.ToList(), this.arguments.Projects.ToList(), skipRestore: true);

                                this.it["should not run restore"] = () => this.processRunner.Verify(x => x.Run(It.IsAny<ProcessStartInfo>()), Times.Never);
                            };
                        };

                        this.context["and pins Sundew.Base version to 5.1 latest prerelease"] = () =>
                        {
                            this.beforeEach = () => this.arguments = new UpdateVerb(new List<PackageId> { new("Sundew.Base", "5.1.*") }, this.arguments.Projects.ToList(), allowPrerelease: true);

                            TestData.SundewCommandLineProject.Assert(x => this.it[$@"should write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(
                                    f => f.WriteAllTextAsync(x.Path, TestData.SundewCommandLineData.MajorMinorPinnedSundewBaseUpdatedSource, CancellationToken.None),
                                    Times.Once));

                            TestData.TransparentMoqProject.Assert(x => this.it[$@"should not write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(
                                        f => f.WriteAllTextAsync(x.Path, It.IsAny<string>(), CancellationToken.None),
                                        Times.Never));

                            TestData.SundewBuildPublishProject.Assert(x => this.it[$@"should write to: {x.Path}"] =
                                () => this.fileSystem?.File.Verify(
                                        f => f.WriteAllTextAsync(x.Path, TestData.SundewPackagingPublishData.MajorMinorPinnedSundewBaseUpdatedSource, CancellationToken.None),
                                        Times.Once));

                            TestData.SundewBasePackageUpdateForSundewPackagingPublish.Assert(x =>
                                this.it[$"should update package: {x.Id} from {x.NuGetVersion} to {x.UpdatedNuGetVersion}"] =
                                    () => this.packageVersionSelectorReporter.Verify(
                                            r => r.PackageUpdateSelected(x.Id, x.NuGetVersion, x.UpdatedNuGetVersion),
                                            Times.Once()));

                            TestData.SundewBasePinnedPrereleasePackageUpdateForSundewCommandLine.Assert(x =>
                                this.it[$"should update package: {x.Id} from {x.NuGetVersion} to {x.UpdatedNuGetVersion}"] =
                                    () => this.packageVersionSelectorReporter.Verify(
                                        r => r.PackageUpdateSelected(x.Id, x.NuGetVersion, x.UpdatedNuGetVersion),
                                        Times.Once()));

                            this.it["should not update any other packages"] = () =>
                                this.packageVersionSelectorReporter.Verify(
                                    r => r.PackageUpdateSelected(
                                        It.Is<string>(x => x != TestData.SundewBasePackage.Id),
                                        It.IsAny<NuGetVersion>(),
                                        It.IsAny<NuGetVersion>()),
                                    Times.Never);

                            this.it["should run restore"] = () => this.processRunner.Verify(x => x.Run(It.IsAny<ProcessStartInfo>()), Times.Once);

                            this.context["and skip-restore is set"] = () =>
                            {
                                this.beforeEach = () => this.arguments = new UpdateVerb(this.arguments.PackageIds.ToList(), this.arguments.Projects.ToList(), skipRestore: true);

                                this.it["should not run restore"] = () => this.processRunner.Verify(x => x.Run(It.IsAny<ProcessStartInfo>()), Times.Never);
                            };
                        };
                    };

                    TestData.SundewCommandLineProject.Assert(x => this.it[$@"should write to: {x.Path}"] =
                                                    () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, x.ExpectedNonPrereleaseUpdatedSource, CancellationToken.None), Times.Once));

                    TestData.TransparentMoqProject.Assert(x => this.it[$@"should write to: {x.Path}"] =
                        () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, x.ExpectedNonPrereleaseUpdatedSource, CancellationToken.None), Times.Once));

                    TestData.SundewBuildPublishProject.Assert(x => this.it[$@"should not write to: {x.Path}"] =
                        () => this.fileSystem?.File.Verify(f => f.WriteAllTextAsync(x.Path, It.IsAny<string>(), CancellationToken.None), Times.Never));

                    TestData.SundewBasePackageUpdateForSundewCommandLine.Assert(x =>
                        this.it[$"should update package: {x.Id} from {x.NuGetVersion} to {x.UpdatedNuGetVersion}"] =
                            () => this.packageVersionSelectorReporter.Verify(r => r.PackageUpdateSelected(x.Id, x.NuGetVersion, x.UpdatedNuGetVersion), Times.Once()));

                    TestData.SundewBuildPublishPackageUpdateForSundewCommandLine.Assert(x =>
                        this.it[$"should update package: {x.Id} from {x.NuGetVersion} to {x.UpdatedNuGetVersion}"] =
                            () => this.packageVersionSelectorReporter.Verify(r => r.PackageUpdateSelected(x.Id, x.NuGetVersion, x.UpdatedNuGetVersion), Times.Once()));

                    this.it["should not update any other packages"] = () =>
                        this.packageVersionSelectorReporter.Verify(
                            r => r.PackageUpdateSelected(
                                It.Is<string>(x =>
                                    x != TestData.SundewBasePackage.Id &&
                                    x != TestData.SundewBuildPublishPackage.Id),
                                It.IsAny<NuGetVersion>(),
                                It.IsAny<NuGetVersion>()),
                            Times.Never);

                    this.it["should run restore"] = () => this.processRunner.Verify(x => x.Run(It.IsAny<ProcessStartInfo>()), Times.Once);
                };
            };
        }

        PackageUpdaterFacade? packageUpdaterFacade;
        IFileSystem? fileSystem;
        UpdateVerb? arguments;
        INuGetPackageVersionFetcher? nuGetPackageVersionFetcher;
        IPackageVersionSelectorReporter? packageVersionSelectorReporter;
        IProcessRunner? processRunner;
    }
}
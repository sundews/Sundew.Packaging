// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AwaitPublishFacade.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.AwaitPublish
{
    using System;
    using System.Diagnostics;
    using System.IO.Abstractions;
    using System.Threading;
    using System.Threading.Tasks;
    using global::NuGet.Common;
    using global::NuGet.Protocol;
    using global::NuGet.Protocol.Core.Types;
    using Sundew.Packaging.Tool.NuGet;

    public class AwaitPublishFacade
    {
        private readonly IFileSystem fileSystem;
        private readonly INuGetSourceProvider nuGetSourceProvider;
        private readonly IAwaitPublishFacadeReporter awaitPublishFacadeReporter;

        public AwaitPublishFacade(IFileSystem fileSystem, INuGetSourceProvider nuGetSourceProvider, IAwaitPublishFacadeReporter awaitPublishFacadeReporter)
        {
            this.fileSystem = fileSystem;
            this.nuGetSourceProvider = nuGetSourceProvider;
            this.awaitPublishFacadeReporter = awaitPublishFacadeReporter;
        }

        public async Task<int> Await(AwaitPublishVerb awaitPublishVerb)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var rootDirectory = awaitPublishVerb.RootDirectory ?? this.fileSystem.Directory.GetCurrentDirectory();
                var (source, _) = this.nuGetSourceProvider.GetSourceSettings(rootDirectory, awaitPublishVerb.Source);

                var logger = NullLogger.Instance;
                var sourceRepository = Repository.Factory.GetCoreV3(source);
                this.awaitPublishFacadeReporter.StartWaitingForPackage(awaitPublishVerb.PackageIdAndVersion, source);
                var resource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(CancellationToken.None)
                    .ConfigureAwait(false);
                var packageExists = false;
                using var cancellationTokenSource = new CancellationTokenSource(awaitPublishVerb.Timeout);
                if (resource != null)
                {
                    while (!packageExists && !cancellationTokenSource.IsCancellationRequested)
                    {
                        packageExists = await resource.DoesPackageExistAsync(
                            awaitPublishVerb.PackageIdAndVersion.Id,
                            awaitPublishVerb.PackageIdAndVersion.NuGetVersion,
                            new NullSourceCacheContext
                            {
                                DirectDownload = true,
                                NoCache = true,
                                RefreshMemoryCache = true,
                            },
                            logger,
                            cancellationTokenSource.Token).ConfigureAwait(false);
                        this.awaitPublishFacadeReporter.PackageExistsResult(awaitPublishVerb.PackageIdAndVersion, packageExists);

                        await Task.Delay(TimeSpan.FromSeconds(2), cancellationTokenSource.Token).ConfigureAwait(false);
                    }
                }

                this.awaitPublishFacadeReporter.CompletedWaitingForPackage(awaitPublishVerb.PackageIdAndVersion, packageExists, stopwatch.Elapsed);
                return packageExists ? 0 : -2;
            }
            catch (OperationCanceledException)
            {
                this.awaitPublishFacadeReporter.CompletedWaitingForPackage(awaitPublishVerb.PackageIdAndVersion, false, stopwatch.Elapsed);
                return -3;
            }
            catch (Exception e)
            {
                this.awaitPublishFacadeReporter.Exception(e);
                return -1;
            }
        }
    }
}
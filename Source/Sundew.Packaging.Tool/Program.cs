// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool
{
    using System;
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using Sundew.Base.Primitives.Computation;
    using Sundew.Base.Primitives.Time;
    using Sundew.CommandLine;
    using Sundew.Packaging.Publish;
    using Sundew.Packaging.Tool.AwaitPublish;
    using Sundew.Packaging.Tool.Delete;
    using Sundew.Packaging.Tool.Diagnostics;
    using Sundew.Packaging.Tool.NuGet;
    using Sundew.Packaging.Tool.PruneLocalSource;
    using Sundew.Packaging.Tool.Update;
    using Sundew.Packaging.Tool.Update.MsBuild.NuGet;
    using Sundew.Packaging.Tool.Versioning;
    using Sundew.Packaging.Tool.Versioning.Logging;
    using Sundew.Packaging.Tool.Versioning.MsBuild;
    using Sundew.Packaging.Versioning.Commands;
    using Sundew.Packaging.Versioning.Logging;

    /// <summary>
    /// The program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The entrance point.
        /// </summary>
        /// <returns>An exit code.</returns>
        public static async Task<int> Main()
        {
            try
            {
                var commandLineParser = new CommandLineParser<int, int>();
                commandLineParser.AddVerb(new StageBuildVerb(), ExecuteGetVersionAsync);
                commandLineParser.AddVerb(new UpdateVerb(), ExecuteUpdateAsync);
                commandLineParser.AddVerb(new AwaitPublishVerb(), ExecuteAwaitPublishAsync);
                commandLineParser.AddVerb(new PruneLocalSourceVerb(), v => Result.Error(ParserError.From(-1)), builder =>
                {
                    builder.AddVerb(new AllVerb(), ExecutePruneAllAsync);
                    //// builder.AddVerb(new NewestPrereleasesPruneModeVerb(), ExecutePruneNewestPrereleasesAsync);
                });
                commandLineParser.AddVerb(new DeleteVerb(), ExecuteDeleteAsync);
                var result = await commandLineParser.ParseAsync(Environment.CommandLine, 1);
                if (!result)
                {
                    result.WriteToConsole();
                }

                return result.GetExitCode();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return -1;
            }
        }

        private static async ValueTask<Result<int, ParserError<int>>> ExecuteGetVersionAsync(StageBuildVerb getVersionVerb)
        {
            var consoleReporter = new ConsoleReporter(false);
            var consoleLogger = new ConsoleLogger();
            var nuGetToLoggerAdapter = new NuGetToLoggerAdapter(consoleLogger);
            var fileSystem = new Sundew.Packaging.Versioning.IO.FileSystem();
            var getVersionFacade = new StageBuildFacade(
                new ProjectPackageInfoProvider(),
                new PackageVersioner(new PackageExistsCommand(nuGetToLoggerAdapter), new LatestPackageVersionCommand(consoleLogger, nuGetToLoggerAdapter), consoleLogger),
                new NuGetSettingsInitializationCommand(),
                new DateTimeProvider(),
                fileSystem,
                new PackageVersionLogger(consoleReporter),
                consoleReporter);
            await getVersionFacade.GetVersionAsync(getVersionVerb);
            return Result.Success(0);
        }

        private static async ValueTask<Result<int, ParserError<int>>> ExecuteDeleteAsync(DeleteVerb deleteVerb)
        {
            var deleteFacade = new DeleteFacade(new FileSystem(), new ConsoleReporter(deleteVerb.Verbose));
            return Result.Success(await deleteFacade.Delete(deleteVerb));
        }

        private static async ValueTask<Result<int, ParserError<int>>> ExecuteAwaitPublishAsync(AwaitPublishVerb awaitPublishVerb)
        {
            var consoleReporter = new ConsoleReporter(awaitPublishVerb.Verbose);
            var awaitPublishFacade = new AwaitPublishFacade(new FileSystem(), new NuGetSourceProvider(), consoleReporter);
            var result = await awaitPublishFacade.Await(awaitPublishVerb);
            return Result.From(result == 0, result, new ParserError<int>(result));
        }

        private static async ValueTask<Result<int, ParserError<int>>> ExecuteUpdateAsync(UpdateVerb updateVerb)
        {
            var consoleReporter = new ConsoleReporter(updateVerb.Verbose);
            var packageUpdaterFacade = new PackageUpdaterFacade(new FileSystem(), new NuGetPackageVersionFetcher(new NuGetSourceProvider()), new ProcessRunner(), consoleReporter, consoleReporter, consoleReporter, consoleReporter);
            await packageUpdaterFacade.UpdatePackagesInProjectsAsync(updateVerb);
            return Result.Success(0);
        }

        private static async ValueTask<Result<int, ParserError<int>>> ExecutePruneAllAsync(AllVerb allVerb)
        {
            var pruneAllFacade = new PruneAllFacade(new NuGetSourceProvider(), new FileSystem(), new ConsoleReporter(allVerb.Verbose));
            await pruneAllFacade.PruneAsync(allVerb);
            return Result.Success(0);
        }

        private static async ValueTask<Result<int, ParserError<int>>> ExecutePruneNewestPrereleasesAsync(NewestPrereleasesPruneModeVerb newestPrereleasesPruneModeVerb)
        {
            var pruneNewestPrereleaseFacade = new NewestPrereleasesPruneFacade(new NuGetSourceProvider());
            await pruneNewestPrereleaseFacade.PruneAsync(newestPrereleasesPruneModeVerb);
            return Result.Success(-1);
        }
    }
}

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Sundew.Base.Primitives.Computation;
using Sundew.Base.Primitives.Time;
using Sundew.CommandLine;
using Sundew.Packaging.Tool.AwaitPublish;
using Sundew.Packaging.Tool.Delete;
using Sundew.Packaging.Tool.Diagnostics;
using Sundew.Packaging.Tool.NuGet;
using Sundew.Packaging.Tool.PruneLocalSource;
using Sundew.Packaging.Tool.Push;
using Sundew.Packaging.Tool.Update;
using Sundew.Packaging.Tool.Update.MsBuild.NuGet;
using Sundew.Packaging.Tool.Versioning;
using Sundew.Packaging.Tool.Versioning.Logging;
using Sundew.Packaging.Tool.Versioning.MsBuild;
using Sundew.Packaging.Versioning;
using Sundew.Packaging.Versioning.Commands;
using Sundew.Packaging.Versioning.Logging;
using Sundew.Packaging.Versioning.NuGet.Configuration;

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
            var commandLineParser = new CommandLineParser<int, string>();
            commandLineParser.AddVerb(new StageBuildVerb(), ExecuteStageBuildAsync);
            commandLineParser.AddVerb(new PushVerb(), ExecutePushAsync);
            commandLineParser.AddVerb(new UpdateVerb(), ExecuteUpdateAsync);
            commandLineParser.AddVerb(new AwaitPublishVerb(), ExecuteAwaitPublishAsync);
            commandLineParser.AddVerb(new PruneLocalSourceVerb(), v => R.Error(ParserError.From("Prune needs sub command.")), builder =>
            {
                builder.AddVerb(new AllVerb(), ExecutePruneAllAsync);
                //// builder.AddVerb(new NewestPrereleasesPruneModeVerb(), ExecutePruneNewestPrereleasesAsync);
            });
            commandLineParser.AddVerb(new DeleteVerb(), ExecuteDeleteAsync);
            var result = await commandLineParser.ParseAsync(Environment.CommandLine, 1);
            if (!result)
            {
                result.WriteToConsole();
                return -1;
            }

            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return -1;
        }
    }

    private static ValueTask<R<int, ParserError<string>>> ExecutePushAsync(PushVerb pushVerb)
    {
        var consoleLogger = new ConsoleLogger();
        return RunSafeAsync(
            () =>
            {
                var attempter = new Attempter(pushVerb.Retries + 1);
                return attempter.AttemptAsync(
                    async (attempt, token) =>
                    {
                        var nuGetToLoggerAdapter = new NuGetToLoggerAdapter(consoleLogger);
                        var pushFacade = new PushFacade(new Sundew.Packaging.Versioning.IO.FileSystem(), new SettingsFactory(), nuGetToLoggerAdapter);
                        await pushFacade.PushAsync(pushVerb);
                    },
                    ExceptionFilter.HandleAll(),
                    failedAttempt =>
                        consoleLogger.LogError(
                            $"Attempt {failedAttempt.Attempt} of {failedAttempt.MaxAttempts} to push package failed with message: {failedAttempt.Exception.Message}"));
            });
    }

    private static ValueTask<R<int, ParserError<string>>> ExecuteStageBuildAsync(StageBuildVerb stageBuildVerb)
    {
        return RunSafeAsync(
            async () =>
            {
                var consoleReporter = new ConsoleReporter(false);
                var consoleLogger = new ConsoleLogger();
                var nuGetToLoggerAdapter = new NuGetToLoggerAdapter(consoleLogger);
                var fileSystem = new Sundew.Packaging.Versioning.IO.FileSystem();
                var stageBuildFacade = new StageBuildFacade(
                    new ProjectPackageInfoProvider(consoleLogger),
                    new PackageVersioner(new PackageExistsCommand(nuGetToLoggerAdapter), new LatestPackageVersionCommand(consoleLogger, nuGetToLoggerAdapter), consoleLogger),
                    new NuGetSettingsInitializationCommand(),
                    new DateTimeProvider(),
                    fileSystem,
                    new PackageVersionLogger(consoleReporter, new Packaging.Versioning.IO.FileSystem()),
                    consoleReporter);
                await stageBuildFacade.GetVersionAsync(stageBuildVerb);
            });
    }

    private static async ValueTask<R<int, ParserError<string>>> ExecuteDeleteAsync(DeleteVerb deleteVerb)
    {
        try
        {
            var deleteFacade = new DeleteFacade(new FileSystem(), new ConsoleReporter(deleteVerb.Verbose));
            return R.Success(await deleteFacade.Delete(deleteVerb));
        }
        catch (Exception exception)
        {
            return R.Error(ParserError.From(exception.ToString()));
        }
    }

    private static async ValueTask<R<int, ParserError<string>>> ExecuteAwaitPublishAsync(AwaitPublishVerb awaitPublishVerb)
    {
        try
        {
            var consoleReporter = new ConsoleReporter(awaitPublishVerb.Verbose);
            var awaitPublishFacade = new AwaitPublishFacade(new FileSystem(), new NuGetSourceProvider(), consoleReporter);
            var result = await awaitPublishFacade.Await(awaitPublishVerb);
            return R.From(result == 0, result, new ParserError<string>($"Await publish error code: {result}"));
        }
        catch (Exception exception)
        {
            return R.Error(ParserError.From(exception.ToString()));
        }
    }

    private static ValueTask<R<int, ParserError<string>>> ExecuteUpdateAsync(UpdateVerb updateVerb)
    {
        return RunSafeAsync(
            async () =>
            {
                var consoleReporter = new ConsoleReporter(updateVerb.Verbose);
                var packageUpdaterFacade = new PackageUpdaterFacade(new FileSystem(), new NuGetPackageVersionFetcher(new NuGetSourceProvider()), new ProcessRunner(), consoleReporter, consoleReporter, consoleReporter, consoleReporter);
                await packageUpdaterFacade.UpdatePackagesInProjectsAsync(updateVerb);
            });
    }

    private static ValueTask<R<int, ParserError<string>>> ExecutePruneAllAsync(AllVerb allVerb)
    {
        return RunSafeAsync(
            async () =>
            {
                var pruneAllFacade = new PruneAllFacade(new NuGetSourceProvider(), new FileSystem(), new ConsoleReporter(allVerb.Verbose));
                await pruneAllFacade.PruneAsync(allVerb);
            });
    }

    private static async ValueTask<R<int, ParserError<string>>> RunSafeAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            return R.Error(ParserError.From(exception.ToString()));
        }

        return R.Success(0);
    }

    private static async ValueTask<R<int, ParserError<int>>> ExecutePruneNewestPrereleasesAsync(NewestPrereleasesPruneModeVerb newestPrereleasesPruneModeVerb)
    {
        var pruneNewestPrereleaseFacade = new NewestPrereleasesPruneFacade(new NuGetSourceProvider());
        await pruneNewestPrereleaseFacade.PruneAsync(newestPrereleasesPruneModeVerb);
        return R.Success(-1);
    }
}
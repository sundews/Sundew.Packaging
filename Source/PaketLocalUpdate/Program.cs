// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PaketLocalUpdate;

using System;
using System.Threading.Tasks;
using Sundew.Base.Primitives.Computation;
using Sundew.CommandLine;
using Sundew.Packaging.Versioning.Commands;

/// <summary>
/// Entry point for plu.
/// </summary>
public static class Program
{
    /// <summary>
    /// Mains the specified arguments.
    /// </summary>
    /// <returns>An async task.</returns>
    public static async Task<int> Main()
    {
        var commandLineParser = new CommandLineParser<int, int>();
        commandLineParser.WithArguments(new Arguments(), Handle);

        var result = await commandLineParser.ParseAsync(Environment.CommandLine, 1);
        if (!result)
        {
            result.WriteToConsole();
            return result.Error.Info;
        }

        return result.Value;
    }

    private static async ValueTask<R<int, ParserError<int>>> Handle(Arguments arg)
    {
        var updateFacade = new UpdateFacade(new NuGetSettingsInitializationCommand());
        await updateFacade.Update(arg);
        Console.WriteLine(string.Empty);
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Attention!");
        Console.WriteLine("Make sure you do not commit your paket.lock file with local dependencies");
        Console.WriteLine("Run paket install to get paket.lock back or make sure you have updated dependencies to non-local versions");
        Console.ForegroundColor = color;
        return R.Success(0);
    }
}
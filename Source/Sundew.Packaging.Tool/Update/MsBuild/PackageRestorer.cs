// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageRestorer.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Update.MsBuild;

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Sundew.Packaging.Tool.Diagnostics;

public class PackageRestorer
{
    private readonly IProcessRunner processRunner;
    private readonly IPackageRestorerReporter packageRestorerReporter;

    public PackageRestorer(IProcessRunner processRunner, IPackageRestorerReporter packageRestorerReporter)
    {
        this.processRunner = processRunner;
        this.packageRestorerReporter = packageRestorerReporter;
    }

    public async Task RestoreAsync(string rootDirectory, bool verbose)
    {
        var process = this.processRunner.Run(new ProcessStartInfo("dotnet", $"restore -v{(verbose ? " n" : " m")}")
        {
            WorkingDirectory = rootDirectory,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
        });

        if (process != null)
        {
            this.packageRestorerReporter.ReportMessage(string.Empty);
            this.packageRestorerReporter.ReportMessage($"Restoring in path: {rootDirectory}");
            while (!process.StandardOutput.EndOfStream)
            {
                var line = await process.StandardOutput.ReadLineAsync();
                if (line != null)
                {
                    this.packageRestorerReporter.ReportMessage(line);
                }
            }

            process.WaitForExit();
        }
    }
}
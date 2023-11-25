// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessRunner.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Diagnostics;

using System.Diagnostics;

public class ProcessRunner : IProcessRunner
{
    public Process? Run(ProcessStartInfo processStartInfo)
    {
        var process = System.Diagnostics.Process.Start(processStartInfo);
        if (process == null)
        {
            return null;
        }

        return new Process(process);
    }
}
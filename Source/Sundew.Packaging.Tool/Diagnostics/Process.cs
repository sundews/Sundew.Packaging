﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Process.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Diagnostics;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class Process : IProcess
{
    private readonly System.Diagnostics.Process process;

    public Process(System.Diagnostics.Process process)
    {
        this.process = process;
    }

    public int ExitCode => this.process.ExitCode;

    public bool HasExited => this.process.HasExited;

    public DateTime StartTime => this.process.StartTime;

    public DateTime ExitTime => this.process.ExitTime;

    public int Id => this.process.Id;

    public string MachineName => this.process.MachineName;

    public StreamReader StandardOutput => this.process.StandardOutput;

    public StreamReader StandardError => this.process.StandardError;

    public StreamWriter StandardInput => this.process.StandardInput;

    public Task WaitForExitAsync(CancellationToken cancellationToken)
    {
        this.process.WaitForExit();
        return Task.CompletedTask;
    }

    public void WaitForExit()
    {
        this.process.WaitForExit();
    }
}
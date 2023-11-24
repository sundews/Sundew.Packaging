// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleLogger.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Versioning.Logging;

using System;
using Sundew.Packaging.Versioning.Logging;

internal class ConsoleLogger : ILogger
{
    public void LogError(string message)
    {
        Console.WriteLine(message);
    }

    public void LogImportant(string message)
    {
        Console.WriteLine(message);
    }

    public void LogInfo(string message)
    {
        Console.WriteLine(message);
    }

    public void LogMessage(string message)
    {
        Console.WriteLine(message);
    }

    public void LogWarning(string message)
    {
        Console.WriteLine(message);
    }
}
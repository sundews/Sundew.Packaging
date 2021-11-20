// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILogger.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.Logging;

/// <summary>Inteface for implementing a command logger.</summary>
public interface ILogger
{
    /// <summary>Logs the important.</summary>
    /// <param name="message">The message.</param>
    void LogImportant(string message);

    /// <summary>Logs the information.</summary>
    /// <param name="message">The message.</param>
    void LogInfo(string message);

    /// <summary>Logs the message.</summary>
    /// <param name="message">The message.</param>
    void LogMessage(string message);

    /// <summary>
    /// Logs the warning.
    /// </summary>
    /// <param name="message">The message.</param>
    void LogWarning(string message);

    /// <summary>
    /// Logs the error.
    /// </summary>
    /// <param name="message">The message.</param>
    void LogError(string message);
}
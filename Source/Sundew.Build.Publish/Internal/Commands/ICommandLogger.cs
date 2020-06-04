﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICommandLogger.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Internal.Commands
{
    /// <summary>Inteface for implementing a command logger.</summary>
    public interface ICommandLogger
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
    }
}
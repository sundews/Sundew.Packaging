// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStageBuildLogger.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Versioning;

using Sundew.Packaging.Tool.Reporting;

public interface IStageBuildLogger : IExceptionReporter
{
    void ReportMessage(string message);
}
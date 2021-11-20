// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAppendPublishFileLogCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands;

using Sundew.Packaging.Versioning;

internal interface IAppendPublishFileLogCommand
{
    /// <summary>
    /// Appends the specified output directory.
    /// </summary>
    /// <param name="workingDirectory">The output directory.</param>
    /// <param name="packagePushFileAppendFormats">The package push file append formats.</param>
    /// <param name="packageId">The package identifier.</param>
    /// <param name="packagePath">The package path.</param>
    /// <param name="symbolPackagePath">The symbol package path.</param>
    /// <param name="publishInfo">The publish information.</param>
    /// <param name="parameter">The parameter.</param>
    void Append(
        string workingDirectory,
        string packagePushFileAppendFormats,
        string packageId,
        string packagePath,
        string? symbolPackagePath,
        PublishInfo publishInfo,
        string parameter);
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISettingsFactory.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.NuGet.Configuration;

using global::NuGet.Configuration;

/// <summary>
/// Factory for getting NuGet settings.
/// </summary>
public interface ISettingsFactory
{
    /// <summary>
    /// Loads the default settings.
    /// </summary>
    /// <param name="root">The root.</param>
    /// <returns>The settings.</returns>
    ISettings LoadDefaultSettings(string root);

    /// <summary>
    /// Loads the specific settings.
    /// </summary>
    /// <param name="root">The root.</param>
    /// <param name="configFileName">Name of the configuration file.</param>
    /// <returns>The settings.</returns>
    ISettings LoadSpecificSettings(string root, string configFileName);

    /// <summary>
    /// Creates setting in the specified root.
    /// </summary>
    /// <param name="root">The root.</param>
    /// <param name="configFileName">Name of the configuration file.</param>
    /// <param name="isMachineWide">if set to <c>true</c> [is machine wide].</param>
    /// <returns>The settings.</returns>
    ISettings Create(string root, string configFileName, bool isMachineWide);

    /// <summary>
    /// Loads the machine wide settings.
    /// </summary>
    /// <param name="root">The root.</param>
    /// <returns>The settings.</returns>
    ISettings LoadMachineWideSettings(string root);
}
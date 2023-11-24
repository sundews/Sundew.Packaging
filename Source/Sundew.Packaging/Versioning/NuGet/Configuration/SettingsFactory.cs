// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsFactory.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.NuGet.Configuration;

using global::NuGet.Configuration;

/// <summary>
/// Factory class for NuGet settings.
/// </summary>
/// <seealso cref="Sundew.Packaging.Versioning.NuGet.Configuration.ISettingsFactory" />
public class SettingsFactory : ISettingsFactory
{
    /// <summary>
    /// Loads the default settings.
    /// </summary>
    /// <param name="root">The root.</param>
    /// <returns>
    /// The settings.
    /// </returns>
    public ISettings LoadDefaultSettings(string root)
    {
        return Settings.LoadDefaultSettings(root);
    }

    /// <summary>
    /// Loads the specific settings.
    /// </summary>
    /// <param name="root">The root.</param>
    /// <param name="configFileName">Name of the configuration file.</param>
    /// <returns>
    /// The settings.
    /// </returns>
    public ISettings LoadSpecificSettings(string root, string configFileName)
    {
        return Settings.LoadSpecificSettings(root, configFileName);
    }

    /// <summary>
    /// Loads the machine wide settings.
    /// </summary>
    /// <param name="root">The root.</param>
    /// <returns>
    /// The settings.
    /// </returns>
    public ISettings LoadMachineWideSettings(string root)
    {
        return Settings.LoadMachineWideSettings(root, new string[0]);
    }

    /// <summary>
    /// Creates setting in the specified root.
    /// </summary>
    /// <param name="root">The root.</param>
    /// <param name="configFileName">Name of the configuration file.</param>
    /// <param name="isMachineWide">if set to <c>true</c> [is machine wide].</param>
    /// <returns>
    /// The settings.
    /// </returns>
    public ISettings Create(string root, string configFileName, bool isMachineWide)
    {
        return new Settings(root, configFileName, isMachineWide);
    }
}
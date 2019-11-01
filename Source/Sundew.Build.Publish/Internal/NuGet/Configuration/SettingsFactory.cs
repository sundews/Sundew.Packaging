// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsFactory.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Internal.NuGet.Configuration
{
    using global::NuGet.Configuration;

    internal class SettingsFactory : ISettingsFactory
    {
        public ISettings LoadDefaultSettings(string root)
        {
            return Settings.LoadDefaultSettings(root);
        }

        public ISettings LoadSpecificSettings(string root, string configFileName)
        {
            return Settings.LoadSpecificSettings(root, configFileName);
        }

        public ISettings Create(string root, string configFileName, bool isMachineWide)
        {
            return new Settings(root, configFileName, isMachineWide);
        }
    }
}
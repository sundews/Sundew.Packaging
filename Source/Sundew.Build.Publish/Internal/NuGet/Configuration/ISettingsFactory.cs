// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISettingsFactory.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Internal.NuGet.Configuration
{
    using global::NuGet.Configuration;

    internal interface ISettingsFactory
    {
        ISettings LoadDefaultSettings(string root);

        ISettings LoadSpecificSettings(string root, string configFileName);

        ISettings Create(string root, string configFileName, bool isMachineWide);
    }
}
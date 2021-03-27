// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPublishInfoProvider.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    internal interface IPublishInfoProvider
    {
        PublishInfo Save(string publishInfoFilePath, SelectedSource selectedSource, string nugetVersion, bool includeSymbols);

        PublishInfo Read(string publishInfoFilePath);

        void Delete(string publishInfoFilePath);
    }
}
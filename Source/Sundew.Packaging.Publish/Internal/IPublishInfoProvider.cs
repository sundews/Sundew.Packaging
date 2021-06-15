// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPublishInfoProvider.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    using Sundew.Packaging.Staging;
    using Sundew.Packaging.Versioning;

    internal interface IPublishInfoProvider
    {
        PublishInfo Save(string publishInfoFilePath, SelectedStage selectedSource, string nuGetVersion, string fullNuGetVersion, string metadata, bool includeSymbols);

        PublishInfo Read(string publishInfoFilePath);

        void Delete(string publishInfoFilePath);
    }
}
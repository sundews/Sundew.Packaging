// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAutomaticPackageVersioner.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Internal
{
    using System.Threading.Tasks;
    using global::NuGet.Common;
    using global::NuGet.Versioning;

    internal interface IAutomaticPackageVersioner
    {
        Task<SemanticVersion> GetSemanticVersion(string packageId, SemanticVersion semanticVersion, string sourceUri, ILogger logger);
    }
}
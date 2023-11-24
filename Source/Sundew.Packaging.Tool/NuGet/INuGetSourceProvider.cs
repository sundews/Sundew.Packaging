// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INuGetSourceProvider.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.NuGet;

public interface INuGetSourceProvider
{
    SourceSettings GetPushSourceSettings(string rootDirectory, string? source);

    SourceSettings GetSourceSettings(string rootDirectory, string? source);

    string GetDefaultSource(string source);
}
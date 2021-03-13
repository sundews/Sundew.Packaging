// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectedSource.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    internal record SelectedSource : Source
    {
        public SelectedSource(Source source, string? packagePrefix = null, string? packagePostfix = null)
          : base(source)
        {
            this.PackagePrefix = packagePrefix ?? string.Empty;
            this.PackagePostfix = packagePostfix ?? string.Empty;
        }

        public string PackagePostfix { get; }

        public string PackagePrefix { get; }
    }
}
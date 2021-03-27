// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrereleaseDateTimeProvider.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    using System;
    using System.Globalization;
    using Sundew.Base.Time;
    using Sundew.Packaging.Publish.Internal.IO;

    internal class PrereleaseDateTimeProvider
    {
        private readonly IFileSystem fileSystem;
        private readonly IDateTime dateTime;

        public PrereleaseDateTimeProvider(IFileSystem fileSystem, IDateTime dateTime)
        {
            this.fileSystem = fileSystem;
            this.dateTime = dateTime;
        }

        public DateTime GetUtcDateTime(string buildDateTimeFilePath)
        {
            if (this.fileSystem.FileExists(buildDateTimeFilePath))
            {
                return DateTime.Parse(this.fileSystem.ReadAllText(buildDateTimeFilePath), CultureInfo.InvariantCulture);
            }

            var dateTime = this.dateTime.UtcTime;
            return dateTime;
        }
    }
}
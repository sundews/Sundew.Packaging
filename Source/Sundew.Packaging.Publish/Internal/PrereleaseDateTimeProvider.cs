// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrereleaseDateTimeProvider.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal;

using System;
using System.Globalization;
using Sundew.Base.Time;
using Sundew.Packaging.Versioning.IO;
using Sundew.Packaging.Versioning.Logging;

internal class PrereleaseDateTimeProvider
{
    internal const string UniversalDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ";
    private readonly IFileSystem fileSystem;
    private readonly IDateTime dateTime;
    private readonly ILogger logger;

    public PrereleaseDateTimeProvider(IFileSystem? fileSystem, ILogger logger)
        : this(fileSystem ?? new FileSystem(), new DateTimeProvider(), logger)
    {
    }

    public PrereleaseDateTimeProvider(IFileSystem fileSystem, IDateTime dateTime, ILogger logger)
    {
        this.fileSystem = fileSystem;
        this.dateTime = dateTime;
        this.logger = logger;
    }

    public DateTime GetBuildDateTime(string buildInfoFilePath)
    {
        if (this.fileSystem.FileExists(buildInfoFilePath))
        {
            var dateTimeText = this.fileSystem.ReadAllText(buildInfoFilePath);
            this.logger.LogInfo($"SPP: Using preset DateTime: {dateTimeText} from {buildInfoFilePath}");
            return DateTime.ParseExact(dateTimeText, UniversalDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        var dateTime = this.dateTime.UtcNow;
        this.logger.LogInfo($"SPP: Using DateTime.UtcNow: {dateTime}");
        return dateTime;
    }

    public DateTime SaveBuildDateTime(string buildInfoFilePath)
    {
        var dateTime = this.dateTime.UtcNow;
        var dateTimeText = dateTime.ToString(UniversalDateTimeFormat, CultureInfo.InvariantCulture);
        this.fileSystem.WriteAllText(buildInfoFilePath, dateTimeText);
        this.logger.LogImportant($"Wrote build time: {dateTimeText} to {buildInfoFilePath}");
        return dateTime;
    }
}
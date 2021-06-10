// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublishInfoProvider.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    using System.IO;
    using Newtonsoft.Json;
    using Sundew.Packaging.Source;
    using Sundew.Packaging.Versioning;
    using Sundew.Packaging.Versioning.IO;
    using Sundew.Packaging.Versioning.Logging;

    internal class PublishInfoProvider : IPublishInfoProvider
    {
        private readonly IFileSystem fileSystem;
        private readonly ILogger logger;

        public PublishInfoProvider(IFileSystem fileSystem, ILogger logger)
        {
            this.fileSystem = fileSystem;
            this.logger = logger;
        }

        public PublishInfo Save(string publishInfoFilePath, SelectedSource selectedSource, string nugetVersion, string nugetVersionNormalized, string metadata, bool includeSymbols)
        {
            var publishInfo = new PublishInfo(
                selectedSource.Stage,
                selectedSource.VersionStage,
                selectedSource.FeedSource,
                selectedSource.PushSource,
                selectedSource.ApiKey,
                includeSymbols ? selectedSource.SymbolsPushSource : null,
                includeSymbols ? selectedSource.SymbolsApiKey : null,
                selectedSource.IsEnabled,
                nugetVersion,
                nugetVersionNormalized,
                metadata);
            var directoryPath = Path.GetDirectoryName(publishInfoFilePath);
            if (!this.fileSystem.DirectoryExists(directoryPath))
            {
                this.fileSystem.CreateDirectory(directoryPath);
            }

            this.fileSystem.WriteAllText(publishInfoFilePath, JsonConvert.SerializeObject(publishInfo));
            this.logger.LogInfo($"Wrote publish info: Stage: {selectedSource.Stage}, Feed: {selectedSource.FeedSource}, PushSource: {selectedSource.PushSource}, IsEnabled: {selectedSource.IsEnabled} to {publishInfoFilePath}");
            return publishInfo;
        }

        public PublishInfo Read(string publishInfoFilePath)
        {
            var publishInfoText = this.fileSystem.ReadAllText(publishInfoFilePath!);
            var publishInfo = JsonConvert.DeserializeObject<PublishInfo>(publishInfoText);
            this.logger.LogInfo($"Read publish info: Stage: {publishInfo.Stage}, Feed: {publishInfo.FeedSource}, PushSource: {publishInfo.PushSource}, IsEnabled: {publishInfo.IsEnabled} from {publishInfoFilePath}");
            return publishInfo;
        }

        public void Delete(string publishInfoFilePath)
        {
            this.fileSystem.DeleteFile(publishInfoFilePath);
        }
    }
}
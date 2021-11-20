﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CopyPdbToSymbolCacheCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using global::NuGet.Configuration;
using Sundew.Packaging.Versioning.IO;
using Sundew.Packaging.Versioning.Logging;

/// <summary>
/// Copies the specified pdb file to the symbol cache.
/// </summary>
/// <seealso cref="ICopyPdbToSymbolCacheCommand" />
public class CopyPdbToSymbolCacheCommand : ICopyPdbToSymbolCacheCommand
{
    internal static readonly string DefaultSymbolCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp\SymbolCache");
    private const string ConfigText = "config";
    private const string ConfiguredSymbolCacheDirText = "symbolCacheDir";
    private const string PdbIdPostFixText = "ffffffff";
    private const string SppFileExtensionText = ".spp";
    private const string SppFileExtensionPatternText = "*.spp";
    private readonly IFileSystem fileSystem;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CopyPdbToSymbolCacheCommand"/> class.
    /// </summary>
    /// <param name="fileSystem">The file system.</param>
    /// <param name="logger">The logger.</param>
    public CopyPdbToSymbolCacheCommand(IFileSystem fileSystem, ILogger logger)
    {
        this.fileSystem = fileSystem;
        this.logger = logger;
    }

    /// <summary>
    /// Adds the specified PDB file path.
    /// </summary>
    /// <param name="pdbFilePaths">The PDB file paths.</param>
    /// <param name="symbolCacheDirectoryPath">The symbol cache directory path.</param>
    /// <param name="settings">The settings.</param>
    public void AddAndCleanCache(IReadOnlyList<string> pdbFilePaths, string? symbolCacheDirectoryPath, ISettings settings)
    {
        if (symbolCacheDirectoryPath == null || string.IsNullOrEmpty(symbolCacheDirectoryPath))
        {
            symbolCacheDirectoryPath = settings.GetSection(ConfigText)?.Items.OfType<AddItem>()
                .FirstOrDefault(x =>
                    x.Key.Equals(ConfiguredSymbolCacheDirText, StringComparison.InvariantCultureIgnoreCase))?.Value ?? DefaultSymbolCachePath;
        }

        foreach (var pdbFilePath in pdbFilePaths)
        {
            var packageSymbolCacheDirectory = Path.Combine(symbolCacheDirectoryPath, Path.GetFileName(pdbFilePath));
            if (this.fileSystem.DirectoryExists(packageSymbolCacheDirectory))
            {
                foreach (var sppFilePath in this.fileSystem.EnumerableFiles(packageSymbolCacheDirectory, SppFileExtensionPatternText, SearchOption.AllDirectories))
                {
                    var sppDirectoryPath = Path.GetDirectoryName(sppFilePath);
                    this.fileSystem.DeleteDirectory(sppDirectoryPath, true);
                    this.logger.LogMessage($"Deleted pdb directory: {sppDirectoryPath}.");
                }
            }
        }

        foreach (var pdbFilePath in pdbFilePaths)
        {
            try
            {
                var debugId = GetDebugIdDirectoryName(this.GetDebugId(pdbFilePath));
                var pdbFileName = Path.GetFileName(pdbFilePath);
                var outputPath = Path.Combine(symbolCacheDirectoryPath, pdbFileName, debugId, pdbFileName);
                var pdbDirectoryPath = Path.GetDirectoryName(outputPath);
                this.fileSystem.CreateDirectory(pdbDirectoryPath);
                this.fileSystem.WriteAllText(Path.Combine(pdbDirectoryPath, SppFileExtensionText), string.Empty);
                this.fileSystem.Copy(pdbFilePath, outputPath, true);
                this.logger.LogInfo($"Successfully copied pdb file to: {outputPath}.");
            }
            catch (BadImageFormatException)
            {
                this.logger.LogWarning("Pdb could not be copied to symbol cache. Only portable pdbs are supported.");
            }
        }
    }

    private static string GetDebugIdDirectoryName(byte[] debugId)
    {
        var stringBuilder = new StringBuilder();
        foreach (var value in debugId)
        {
            stringBuilder.AppendFormat($"{value:X2}");
        }

        stringBuilder.Append(PdbIdPostFixText);
        return stringBuilder.ToString();
    }

    private byte[] GetDebugId(string pdbFilePath)
    {
        var metadataReaderProvider = MetadataReaderProvider.FromPortablePdbImage(this.fileSystem.ReadAllBytes(pdbFilePath).ToImmutableArray());
        var metadataReader = metadataReaderProvider.GetMetadataReader();
        var pdbId = metadataReader.DebugMetadataHeader!.Id.ToArray();
        var debugId = new byte[16];
        debugId[0] = pdbId[3];
        debugId[1] = pdbId[2];
        debugId[2] = pdbId[1];
        debugId[3] = pdbId[0];
        debugId[4] = pdbId[5];
        debugId[5] = pdbId[4];
        debugId[6] = pdbId[7];
        debugId[7] = pdbId[6];
        debugId[8] = pdbId[8];
        debugId[9] = pdbId[9];
        debugId[10] = pdbId[10];
        debugId[11] = pdbId[11];
        debugId[12] = pdbId[12];
        debugId[13] = pdbId[13];
        debugId[14] = pdbId[14];
        debugId[15] = pdbId[15];

        return debugId;
    }
}
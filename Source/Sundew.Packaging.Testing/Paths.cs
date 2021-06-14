// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Paths.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Testing
{
    using System;
    using System.Text.RegularExpressions;

    public static class Paths
    {
        private const string Backslash = "\\";
        private const string Forwardslash = "/";
        private const string Colon = ":";

        private static readonly Regex WindowsToUnixPathRegex = new(@"^\w\:|\\");

        public static string EnsurePlatformPath(string path)
        {
            var platform = Environment.OSVersion.Platform;
            if (platform == PlatformID.Win32NT)
            {
                return Regex.Replace(path, Forwardslash, Backslash);
            }

            return WindowsToUnixPathRegex.Replace(
                path,
                m =>
                {
                    if (m.Value.EndsWith(Colon))
                    {
                        return Backslash;
                    }

                    return Forwardslash;
                });
        }
    }
}

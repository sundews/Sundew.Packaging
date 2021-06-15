// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageRestorerReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Update.MsBuild
{
    public interface IPackageRestorerReporter
    {
        void ReportMessage(string message);
    }
}
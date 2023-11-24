// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IProcessRunner.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Diagnostics;

using System.Diagnostics;

public interface IProcessRunner
{
    Process? Run(ProcessStartInfo processStartInfo);
}
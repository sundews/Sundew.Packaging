// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExceptionReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Reporting
{
    using System;

    public interface IExceptionReporter
    {
        void Exception(Exception exception);
    }
}
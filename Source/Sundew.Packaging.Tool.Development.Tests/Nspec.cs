﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Nspec.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Development.Tests;

using System.Linq;
using AwesomeAssertions;
using NSpec;
using NSpec.Domain;
using NSpec.Domain.Formatters;
using NUnit.Framework;

[TestFixture]
public abstract class Nspec : global::NSpec.nspec
{
    [Test]
    public void debug()
    {
        var currentSpec = this.GetType();
        var finder = new SpecFinder(new[] { currentSpec });
        var filter = new Tags().Parse(currentSpec.Name);
        var builder = new ContextBuilder(finder, filter, new DefaultConventions());
        var runner = new ContextRunner(filter, new ConsoleFormatter(), false);
        var results = runner.Run(builder.Contexts().Build());

        // assert that there aren't any failures
        results.Failures().Count().Should().Be(0);
    }
}
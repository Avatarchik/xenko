﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using NUnit.Framework;

using SiliconStudio.Paradox.UI.Controls;

namespace SiliconStudio.Paradox.UI.Tests.Layering
{
    /// <summary>
    /// A class that contains test functions for layering of the <see cref="ToggleButton"/> class.
    /// </summary>
    [TestFixture]
    [System.ComponentModel.Description("Tests for ToggleButton layering")]
    public class ToggleButtonTests : ToggleButton
    {
        /// <summary>
        /// Test the invalidations generated object property changes.
        /// </summary>
        [Test]
        public void TestBasicInvalidations()
        {
            // CheckedImage, IndeterminateImage, UncheckedImage and State are not tested because they can potentially invalidate or not the layout states

            // - test the properties that are not supposed to invalidate the object layout state
            UIElementLayeringTests.TestNoInvalidation(this, () => IsThreeState = true);
        }
    }
}
﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.UI
{
    /// <summary>
    /// Indicates where an element should be displayed on the horizontal axis relative to the allocated layout slot of the parent element.
    /// </summary>
    public enum HorizontalAlignment
    {
        /// <summary>
        /// An element aligned to the left of the layout slot for the parent element.
        /// </summary>
        Left,
        /// <summary>
        /// An element aligned to the center of the layout slot for the parent element.
        /// </summary>
        Center,
        /// <summary>
        /// An element aligned to the right of the layout slot for the parent element.
        /// </summary>
        Right,
        /// <summary>
        /// An element stretched to fill the entire layout slot of the parent element.
        /// </summary>
        Stretch,
    }
}
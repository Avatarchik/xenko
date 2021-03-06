// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>	
    /// Identify which components of each pixel of a render target are writable during blending.	
    /// </summary>	
    /// <remarks>	
    /// These flags can be combined with a bitwise OR and is used in <see cref="BlendState"/>.
    /// </remarks>	
    [Flags]
    [DataContract]
    public enum ColorWriteChannels
    {
        /// <summary>	
        /// None of the data are stored.	
        /// </summary>	
        None = 0,

        /// <summary>	
        /// Allow data to be stored in the red component. 	
        /// </summary>	
        Red = 1,

        /// <summary>	
        /// Allow data to be stored in the green component. 	
        /// </summary>	
        Green = 2,

        /// <summary>	
        /// Allow data to be stored in the blue component. 	
        /// </summary>	
        Blue = 4,

        /// <summary>	
        /// Allow data to be stored in the alpha component. 	
        /// </summary>	
        Alpha = 8,

        /// <summary>	
        /// Allow data to be stored in all components. 	
        /// </summary>	
        All = Alpha | Blue | Green | Red,
    }
}
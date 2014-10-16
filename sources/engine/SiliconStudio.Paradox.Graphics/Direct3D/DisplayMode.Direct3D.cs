﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D 

using SharpDX.DXGI;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class DisplayMode
    {
        internal ModeDescription ToDescription()
        {
            return new ModeDescription(Width, Height, RefreshRate.ToSharpDX(), format: (SharpDX.DXGI.Format)Format);
        }

        internal static DisplayMode FromDescription(ModeDescription description)
        {
            return new DisplayMode((PixelFormat)description.Format, description.Width, description.Height, new Rational(description.RefreshRate.Numerator, description.RefreshRate.Denominator));
        }
    }
}
#endif
﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D 
// Copyright (c) 2010-2012 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Features supported by a <see cref="GraphicsDevice"/>.
    /// </summary>
    /// <remarks>
    /// This class gives also features for a particular format, using the operator this[dxgiFormat] on this structure.
    /// </remarks>
    public partial struct GraphicsDeviceFeatures
    {
        private readonly static List<SharpDX.DXGI.Format> ObsoleteFormatToExcludes = new List<SharpDX.DXGI.Format>() { Format.R1_UNorm, Format.B5G6R5_UNorm, Format.B5G5R5A1_UNorm };

        internal GraphicsDeviceFeatures(GraphicsDevice deviceRoot)
        {
            var nativeDevice = deviceRoot.NativeDevice;

            mapFeaturesPerFormat = new FeaturesPerFormat[256];

            // Set back the real GraphicsProfile that is used
            Profile = GraphicsProfileHelper.FromFeatureLevel(nativeDevice.FeatureLevel);

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            IsProfiled = PixHelper.IsCurrentlyProfiled;
#else
            IsProfiled = false;
#endif

            HasComputeShaders = nativeDevice.CheckFeatureSupport(Feature.ComputeShaders);
            HasDoublePrecision = nativeDevice.CheckFeatureSupport(Feature.ShaderDoubles);
            nativeDevice.CheckThreadingSupport(out HasMultiThreadingConcurrentResources, out this.HasDriverCommandLists);

            // Check features for each DXGI.Format
            foreach (var format in Enum.GetValues(typeof(SharpDX.DXGI.Format)))
            {
                var dxgiFormat = (SharpDX.DXGI.Format)format;
                var maximumMSAA = MSAALevel.None;
                var computeShaderFormatSupport = ComputeShaderFormatSupport.None;
                var formatSupport = FormatSupport.None;

                if (!ObsoleteFormatToExcludes.Contains(dxgiFormat))
                {
                    maximumMSAA = GetMaximumMSAASampleCount(nativeDevice, dxgiFormat);
                    if (HasComputeShaders)
                        computeShaderFormatSupport = nativeDevice.CheckComputeShaderFormatSupport(dxgiFormat);

                    formatSupport = (FormatSupport)nativeDevice.CheckFormatSupport(dxgiFormat);
                }

                //mapFeaturesPerFormat[(int)dxgiFormat] = new FeaturesPerFormat((PixelFormat)dxgiFormat, maximumMSAA, computeShaderFormatSupport, formatSupport);
                mapFeaturesPerFormat[(int)dxgiFormat] = new FeaturesPerFormat((PixelFormat)dxgiFormat, maximumMSAA, formatSupport);
            }
        }

        /// <summary>
        /// Gets the maximum MSAA sample count for a particular <see cref="PixelFormat" />.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="pixelFormat">The pixelFormat.</param>
        /// <returns>The maximum multisample count for this pixel pixelFormat</returns>
        private static MSAALevel GetMaximumMSAASampleCount(SharpDX.Direct3D11.Device device, SharpDX.DXGI.Format pixelFormat)
        {
            int maxCount = 1;
            for (int i = 1; i <= 8; i *= 2)
            {
                if (device.CheckMultisampleQualityLevels(pixelFormat, i) != 0)
                    maxCount = i;
            }
            return (MSAALevel)maxCount;
        }
    }
}
#endif
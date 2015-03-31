﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.GeometricPrimitives;

namespace SiliconStudio.Paradox.Engine.Graphics.ProceduralModels
{
    /// <summary>
    /// A sphere procedural model.
    /// </summary>
    [DataContract("GeoSphereProceduralModel")]
    [Display("GeoSphere")]
    public class GeoSphereProceduralModel : PrimitiveProceduralModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoSphereProceduralModel"/> class.
        /// </summary>
        public GeoSphereProceduralModel()
        {
            Diameter = 100.0f;
            Tessellation = 3;
            UVScales = new Vector2(1);
        }

        /// <summary>
        /// Gets or sets the diameter of this sphere.
        /// </summary>
        /// <value>The diameter.</value>
        [DataMember(10)]
        [DefaultValue(100.0f)]
        public float Diameter { get; set; }

        /// <summary>
        /// Gets or sets the tessellation factor (default: 3.0)
        /// </summary>
        /// <value>The tessellation.</value>
        [DataMember(20)]
        [DefaultValue(3)]
        public int Tessellation { get; set; }

        /// <summary>
        /// Gets or sets the UV scales.
        /// </summary>
        /// <value>The UV scales</value>
        /// <userdoc>The scales to apply to the UV coordinates of the plane.</userdoc>
        [DataMember(30)]
        [DefaultValue(1.0f)]
        [Display("UV Scales")]
        public Vector2 UVScales { get; set; }

        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            return GeometricPrimitive.GeoSphere.New(Diameter, Tessellation, UVScales.X, UVScales.Y);
        }
    }
}
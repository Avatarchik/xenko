﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.IO;

namespace SiliconStudio.Assets.Serializers
{
    public interface IAssetSerializerFactory
    {
        IAssetSerializer TryCreate(string assetFileExtension);
    }

    public interface IAssetSerializer
    {
        object Load(Stream stream, string assetFileExtension);

        void Save(Stream stream, object asset);
    }
}
﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Core.IO
{
    /// <summary>
    /// File mode equivalent of <see cref="System.IO.FileMode"/>.
    /// </summary>
    public enum VirtualFileMode
    {
        /// <summary>
        /// Creates a new file. The function fails if a specified file exists.
        /// </summary>
        CreateNew = 1,

        /// <summary>
        /// Creates a new file, always.
        /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes,
        /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
        /// </summary>
        Create = 2,

        /// <summary>
        /// Opens a file. The function fails if the file does not exist.
        /// </summary>
        Open = 3,

        /// <summary>
        /// Opens a file, always.
        /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
        /// </summary>
        OpenOrCreate = 4,

        /// <summary>
        /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
        /// The calling process must open the file with the GENERIC_WRITE access right.
        /// </summary>
        Truncate = 5,
    }
}
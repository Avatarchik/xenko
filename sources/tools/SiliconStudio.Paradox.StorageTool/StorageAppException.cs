﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox.StorageTool
{
    /// <summary>
    /// Class StorageAppException.
    /// </summary>
    public class StorageAppException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StorageAppException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public StorageAppException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageAppException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public StorageAppException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
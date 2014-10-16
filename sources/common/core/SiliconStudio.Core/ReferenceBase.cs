﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading;

namespace SiliconStudio.Core
{
    /// <summary>
    /// Base class for a <see cref="IReferencable"/> class.
    /// </summary>
    public abstract class ReferenceBase : IReferencable
    {
        private int counter = 1;

        /// <inheritdoc/>
        public int ReferenceCount { get { return counter; } }

        /// <inheritdoc/>
        public virtual int AddReference()
        {
            int newCounter = Interlocked.Increment(ref counter);
            if (newCounter <= 1) throw new InvalidOperationException(FrameworkResources.AddReferenceError);
            return newCounter;
        }

        /// <inheritdoc/>
        public virtual int Release()
        {
            int newCounter = Interlocked.Decrement(ref counter);
            if (newCounter == 0)
            {
                try
                {
                    Destroy();
                } 
                finally 
                {
                    // Reverse back the counter if there are any exceptions in the destroy method
                    Interlocked.Exchange(ref counter, newCounter + 1);
                }
            } 
            else if (newCounter < 0)
                throw new InvalidOperationException(FrameworkResources.ReleaseReferenceError);
            return newCounter;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        protected abstract void Destroy();
    }
}
﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Net;
using System.Windows.Data;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.Shaders.Compiler;
using System.Runtime.CompilerServices;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles.Components
{
    /// <summary>
    /// This <see cref="ParticleSystemComponentRenderer"/> is responsible for preparing and rendering the particles for a specific pass.
    /// </summary>

    class ParticleSystemComponentRenderer : EntityComponentRendererBase
    {
        // TODO For now try to render particle systems as Sprites, later move on to a proper particle representation

        /// <summary>
        /// ParticleBatch batches similar particles together, based on Material properties.
        /// </summary>
        private ParticleBatch particleBatch;

        private ParticleSystemProcessor particleSystemProcessor;

        protected override void InitializeCore()
        {
            base.InitializeCore();

            particleBatch = new ParticleBatch(Context.GraphicsDevice);
        }

        protected override void Unload()
        {
            particleBatch.Dispose();

            base.Unload();
        }

        protected override void PrepareCore(RenderContext context, RenderItemCollection opaqueList, RenderItemCollection transparentList)
        {
            // Early out if particle system processor doesn't exist
            particleSystemProcessor = SceneInstance.GetProcessor<ParticleSystemProcessor>();
            if (particleSystemProcessor == null)
            {
                return;
            }

            // Early out if camera doesn't exist
            var camera = context.GetCurrentCamera();
            if (camera == null)
            {
                return;
            }

            var viewProjectionMatrix = camera.ViewProjectionMatrix;

            foreach (var particleSystemState in particleSystemProcessor.ParticleSystems)
            {
                var sprite = particleSystemState.ParticleSystemComponent.CurrentSprite;
                if (sprite == null || sprite.Texture == null || sprite.Region.Width <= 0f || sprite.Region.Height <= 0f)
                    continue;

                // Perform culling on group and accept
                // TODO Should culling be performed on a per-sprite basis or batched?
                if (!CurrentCullingMask.Contains(particleSystemState.ParticleSystemComponent.Entity.Group))
                    continue;

                // Project the position to find depth for sorting
                var worldPosition = new Vector4(particleSystemState.TransformComponent.WorldMatrix.TranslationVector, 1.0f);
                Vector4 projectedPosition;
                Vector4.Transform(ref worldPosition, ref viewProjectionMatrix, out projectedPosition);
                var projectedZ = projectedPosition.Z / projectedPosition.W;

                var list = sprite.IsTransparent ? transparentList : opaqueList;

                // TODO Sort value based on custom key
                list.Add(new RenderItem(this, particleSystemState, projectedZ));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static private bool IsSameState(ref BlendState oldBlendState, ref DepthStencilState oldDepthStencilState, ref Effect oldEffect,
            BlendState currentBlendState, DepthStencilState currentDepthStencilState, Effect currentEffect)
        {
            bool isSameState = !(oldBlendState != currentBlendState || oldDepthStencilState != currentDepthStencilState || oldEffect != currentEffect);

            oldBlendState = currentBlendState;
            oldDepthStencilState = currentDepthStencilState;
            oldEffect = currentEffect;

            return isSameState;
        }

        protected override void DrawCore(RenderContext context, RenderItemCollection renderItems, int fromIndex, int toIndex)
        {
            var viewParameters = context.Parameters;
            var device = context.GraphicsDevice;
            // var viewProjection = viewParameters.Get(TransformationKeys.ViewProjection);

            var viewMat = viewParameters.Get(TransformationKeys.View);
            var projMat = viewParameters.Get(TransformationKeys.Projection);

//            var viewInv = viewParameters.Get(TransformationKeys.ViewInverse);
            Matrix viewInv;
            Matrix.Invert(ref viewMat, out viewInv);

            // For batching similar materials together
            BlendState previousBlendState = null;
            // TODO Use pre-multiplied alpha-additive blending for particles to reduce blend states (ideally only 1 state should exist) - BlendStateDescription
            DepthStencilState previousDepthStencilState = null; // DepthStencilStateDescription - depth write, depth test, face culling, stencil buffer settings etc.
            Effect previousEffect = null;

            // TODO For now isPicking is being ignored
            // TODO For now SpriteType.Billboard is being ignored
            // TODO Ignore depth variable is ignore (doesn't exist yet) so the depth state is none

            bool hasBegun = false;
            for (var i = fromIndex; i <= toIndex; i++)
            {
                var renderItem = renderItems[i];
                var particleSystemState = (ParticleSystemProcessor.ParticleSystemComponentState)renderItem.DrawContext;
                var particleSystemComponent = particleSystemState.ParticleSystemComponent;
                var sprite = particleSystemComponent.CurrentSprite;
                if (sprite == null)
                    continue;

                var transformComponent = particleSystemState.TransformComponent;
                var depthStencilState = device.DepthStencilStates.None; // Ignore depth

                // Code copied from the sprite component renderer:
                // var blendState = isPicking ? device.BlendStates.Opaque : renderItems.HasTransparency ? (spriteComp.PremultipliedAlpha ? device.BlendStates.AlphaBlend : device.BlendStates.NonPremultiplied) : device.BlendStates.Opaque;
                var blendState = device.BlendStates.AlphaBlend; // TODO: Alpha-additive

                // Code copied from the sprite component renderer
                // var currentEffect = isPicking ? GetOrCreatePickingSpriteEffect() : spriteComp.Tags.Get(IsEntitySelected) ? GetOrCreateSelectedSpriteEffect() : null; // TODO remove this code when material are available
                Effect currentEffect = null; // Is neither picking nor selected - for now

                // Update the sprite batch
                if (!IsSameState(ref previousBlendState, ref previousDepthStencilState, ref previousEffect, blendState, depthStencilState, currentEffect) || !hasBegun)
                {
                    if (hasBegun)
                    {
                        particleBatch.End();
                    }
                    particleBatch.Begin(viewMat, projMat, viewInv, SpriteSortMode.Deferred, blendState, null, depthStencilState, device.RasterizerStates.CullNone, currentEffect);
                    hasBegun = true;
                }

                var sourceRegion = sprite.Region;
                var sourceTexture = sprite.Texture;
                var color = particleSystemComponent.Color; // Or white
                if (sourceTexture == null)
                    continue;

                // Test - draw all particles
                foreach (var emitter in particleSystemComponent.ParticleSystem.Emitters)
                {
                    particleBatch.Draw(sourceTexture, emitter);


                }

            }

            if (hasBegun)
                particleBatch.End();
        }


        protected override void PreDrawCore(RenderContext context)
        {
            base.PreDrawCore(context);
            // Custom pre draw code
        }

        protected override void PostDrawCore(RenderContext context)
        {
            base.PostDrawCore(context);
            // Custom post draw code
        }
    }
}

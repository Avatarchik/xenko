﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Modules;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Physics;
using SiliconStudio.Paradox.Threading;

namespace SiliconStudio.Paradox.Engine
{
    public class PhysicsProcessor : EntityProcessor<PhysicsProcessor.AssociatedData>
    {
        public class AssociatedData
        {
            public PhysicsComponent PhysicsComponent;
            public TransformationComponent TransformationComponent;
            public ModelComponent ModelComponent; //not mandatory, could be null e.g. invisible triggers
        }

        readonly FastList<PhysicsElement> mElements = new FastList<PhysicsElement>();
        readonly FastList<PhysicsElement> mCharacters = new FastList<PhysicsElement>();

        Bullet2PhysicsSystem mPhysicsSystem;
        RenderSystem mRenderSystem;

        public PhysicsProcessor()
            : base(new PropertyKey[] { PhysicsComponent.Key, TransformationComponent.Key })
        {
        }

        protected override AssociatedData GenerateAssociatedData(Entity entity)
        {
            return new AssociatedData
            {
                PhysicsComponent = entity.Get(PhysicsComponent.Key),
                TransformationComponent = entity.Get(TransformationComponent.Key),
                ModelComponent = entity.Get(ModelComponent.Key),
            };
        }

        //This is called by the physics engine to update the transformation of Dynamic rigidbodies
        static void RigidBodySetWorldTransform(PhysicsElement element, Matrix physicsTransform)
        {
            element.UpdateTransformationComponent(physicsTransform);
        }

        //This is valid for Dynamic rigidbodies (called once at initialization) 
        //and Kinematic rigidbodies (called every simulation tick (if body not sleeping) to let the physics engine know where the kinematic body is)
        static void RigidBodyGetWorldTransform(PhysicsElement element, out Matrix physicsTransform)
        {
            physicsTransform = element.DerivePhysicsTransformation();
        }

        void NewElement(PhysicsElement element, AssociatedData data, Entity entity)
        {
            if (element.Shape == null) return; //no shape no purpose

            var shape = element.Shape.Shape;

            element.Data = data;
            element.BoneIndex = -1;

            if (!element.Sprite && element.LinkedBoneName != null && data.ModelComponent != null)
            {
                //find the linked bone, if can't be found we just skip this element
                for (var index = 0; index < data.ModelComponent.ModelViewHierarchy.Nodes.Length; index++)
                {
                    var node = data.ModelComponent.ModelViewHierarchy.Nodes[index];
                    if (node.Name != element.LinkedBoneName) continue;
                    element.BoneIndex = index;
                    break;
                }

                if (element.BoneIndex == -1) return;
            }

            var defaultGroups = element.CanCollideWith == 0 || element.CollisionGroup == 0;

            switch (element.Type)
            {
                case PhysicsElement.Types.PhantomCollider:
                    {
                        var c = mPhysicsSystem.PhysicsEngine.CreateCollider(shape);

                        element.Collider = c; //required by the next call
                        element.Collider.EntityObject = entity; //required by the next call
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        c.IsTrigger = true;

                        if (defaultGroups)
                        {
                            mPhysicsSystem.PhysicsEngine.AddCollider(c);
                        }
                        else
                        {
                            mPhysicsSystem.PhysicsEngine.AddCollider(c, (CollisionFilterGroups)element.CollisionGroup, element.CanCollideWith);
                        }
                    }
                    break;
                case PhysicsElement.Types.StaticCollider:
                    {
                        var c = mPhysicsSystem.PhysicsEngine.CreateCollider(shape);

                        element.Collider = c; //required by the next call
                        element.Collider.EntityObject = entity; //required by the next call
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        c.IsTrigger = false;

                        if (defaultGroups)
                        {
                            mPhysicsSystem.PhysicsEngine.AddCollider(c);
                        }
                        else
                        {
                            mPhysicsSystem.PhysicsEngine.AddCollider(c, (CollisionFilterGroups)element.CollisionGroup, element.CanCollideWith);
                        }
                    }
                    break;
                case PhysicsElement.Types.StaticRigidBody:
                    {
                        var rb = mPhysicsSystem.PhysicsEngine.CreateRigidBody(shape);

                        rb.EntityObject = entity;
                        rb.GetWorldTransformCallback = (out Matrix transform) => RigidBodyGetWorldTransform(element, out transform);
                        rb.SetWorldTransformCallback = transform => RigidBodySetWorldTransform(element, transform);
                        element.Collider = rb;
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        rb.Type = RigidBodyTypes.Static;

                        if (defaultGroups)
                        {
                            mPhysicsSystem.PhysicsEngine.AddRigidBody(rb);
                        }
                        else
                        {
                            mPhysicsSystem.PhysicsEngine.AddRigidBody(rb, (CollisionFilterGroups)element.CollisionGroup, element.CanCollideWith);
                        }
                    }
                    break;
                case PhysicsElement.Types.DynamicRigidBody:
                    {
                        var rb = mPhysicsSystem.PhysicsEngine.CreateRigidBody(shape);

                        rb.EntityObject = entity;
                        rb.GetWorldTransformCallback = (out Matrix transform) => RigidBodyGetWorldTransform(element, out transform);
                        rb.SetWorldTransformCallback = transform => RigidBodySetWorldTransform(element, transform);
                        element.Collider = rb;
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        rb.Type = RigidBodyTypes.Dynamic;
                        rb.Mass = 1.0f;

                        if (defaultGroups)
                        {
                            mPhysicsSystem.PhysicsEngine.AddRigidBody(rb);
                        }
                        else
                        {
                            mPhysicsSystem.PhysicsEngine.AddRigidBody(rb, (CollisionFilterGroups)element.CollisionGroup, element.CanCollideWith);
                        }
                    }
                    break;
                case PhysicsElement.Types.KinematicRigidBody:
                    {
                        var rb = mPhysicsSystem.PhysicsEngine.CreateRigidBody(shape);

                        rb.EntityObject = entity;
                        rb.GetWorldTransformCallback = (out Matrix transform) => RigidBodyGetWorldTransform(element, out transform);
                        rb.SetWorldTransformCallback = transform => RigidBodySetWorldTransform(element, transform);
                        element.Collider = rb;
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        rb.Type = RigidBodyTypes.Kinematic;
                        rb.Mass = 0.0f;

                        if (defaultGroups)
                        {
                            mPhysicsSystem.PhysicsEngine.AddRigidBody(rb);
                        }
                        else
                        {
                            mPhysicsSystem.PhysicsEngine.AddRigidBody(rb, (CollisionFilterGroups)element.CollisionGroup, element.CanCollideWith);
                        }
                    }
                    break;
                case PhysicsElement.Types.CharacterController:
                    {
                        var ch = mPhysicsSystem.PhysicsEngine.CreateCharacter(shape, element.StepHeight);

                        element.Collider = ch;
                        element.Collider.EntityObject = entity;
                        element.UpdatePhysicsTransformation(); //this will set position and rotation of the collider

                        if (defaultGroups)
                        {
                            mPhysicsSystem.PhysicsEngine.AddCharacter(ch);
                        }
                        else
                        {
                            mPhysicsSystem.PhysicsEngine.AddCharacter(ch, (CollisionFilterGroups)element.CollisionGroup, element.CanCollideWith);
                        }

                        mCharacters.Add(element);
                    }
                    break;
            }

            mElements.Add(element);
        }

        void DeleteElement(PhysicsElement element, bool now = false)
        {
            //might be possible that this element was not valid during creation so it would be already null
            if (element.Collider == null) return;

            var toDispose = new List<IDisposable>();

            mElements.Remove(element);   

            switch (element.Type)
            {
                case PhysicsElement.Types.PhantomCollider:
                case PhysicsElement.Types.StaticCollider:
                {
                    mPhysicsSystem.PhysicsEngine.RemoveCollider(element.Collider);
                }
                    break;
                case PhysicsElement.Types.StaticRigidBody:
                case PhysicsElement.Types.DynamicRigidBody:
                case PhysicsElement.Types.KinematicRigidBody:
                {
                    var rb = (RigidBody)element.Collider;
                    var constraints = rb.LinkedConstraints.ToArray();
                    foreach (var c in constraints)
                    {
                        mPhysicsSystem.PhysicsEngine.RemoveConstraint(c);
                        toDispose.Add(c);
                    }

                    mPhysicsSystem.PhysicsEngine.RemoveRigidBody(rb);
                }
                    break;
                case PhysicsElement.Types.CharacterController:
                {
                    mCharacters.Remove(element);
                    mPhysicsSystem.PhysicsEngine.RemoveCharacter((Character) element.Collider);
                }
                    break;
            }

            toDispose.Add(element.Collider);
            element.Collider = null;

            //dispose in another thread for better performance
            if (!now)
            {
                TaskList.Dispatch(toDispose, 4, 128, (i, disposable) => disposable.Dispose());
            }
            else
            {
                foreach (var d in toDispose)
                {
                    d.Dispose();
                }
            }
        }

        protected override void OnEntityAdding(Entity entity, AssociatedData data)
        {
            if (!mPhysicsSystem.PhysicsEngine.Initialized) return;

            foreach (var element in data.PhysicsComponent.Elements)
            {
                NewElement(element, data, entity);
            }
        }

        protected override void OnEntityRemoved(Entity entity, AssociatedData data)
        {
            if (!mPhysicsSystem.PhysicsEngine.Initialized) return;

            foreach (var element in data.PhysicsComponent.Elements)
            {
                DeleteElement(element, true);
            }
        }

        protected internal override void OnEnabledChanged(Entity entity, bool enabled)
        {
            if (!mPhysicsSystem.PhysicsEngine.Initialized) return;

            var elements = entity.Get(PhysicsComponent.Key).Elements;

            foreach (var element in elements.Where(element => element.Collider != null))
            {
                element.Collider.Enabled = enabled;
            }
        }

        protected internal override void OnSystemAdd()
        {
            mPhysicsSystem = (Bullet2PhysicsSystem)Services.GetSafeServiceAs<IPhysicsSystem>();
            mRenderSystem = Services.GetSafeServiceAs<RenderSystem>();

            //setup debug device and debug shader
            var gfxDevice = Services.GetSafeServiceAs<IGraphicsDeviceService>();
            mPhysicsSystem.PhysicsEngine.DebugGraphicsDevice = gfxDevice.GraphicsDevice;

            //Debug primitives render, should happen about the last steps of the pipeline
            mRenderSystem.Pipeline.EndPass += DebugShapesDraw;
        }

        protected internal override void OnSystemRemove()
        {
            //remove all elements from the engine
            foreach (var element in mElements)
            {
                DeleteElement(element);
            }
        }

        private void DebugShapesDraw(RenderContext context)
        {
            if (!mPhysicsSystem.PhysicsEngine.CreateDebugPrimitives ||
                    !mPhysicsSystem.PhysicsEngine.RenderDebugPrimitives || 
                    !mPhysicsSystem.PhysicsEngine.Initialized ||
                    mPhysicsSystem.PhysicsEngine.DebugGraphicsDevice == null ||
                    mPhysicsSystem.PhysicsEngine.DebugEffect == null) 
                return;

            Matrix viewProj;
            if (mRenderSystem.Pipeline.Parameters.ContainsKey(TransformationKeys.View) && mRenderSystem.Pipeline.Parameters.ContainsKey(TransformationKeys.Projection))
            {
                viewProj = mRenderSystem.Pipeline.Parameters.Get(TransformationKeys.View) * mRenderSystem.Pipeline.Parameters.Get(TransformationKeys.Projection);
            }
            else
            {
                return;
            }

            foreach (var element in mElements)
            {
                var shape = element.Shape.Shape;

                if (shape.Type == ColliderShapeTypes.Compound) //multiple shapes
                {
                    var compound = (CompoundColliderShape)shape;
                    for (var i = 0; i < compound.Count; i++)
                    {
                        var subShape = compound[i];
                        if (subShape.Type == ColliderShapeTypes.Compound || subShape.Type == ColliderShapeTypes.StaticPlane) continue;

                        var physTrans = element.BoneIndex == -1 ? element.Collider.PhysicsWorldTransform : element.BoneWorldMatrix;
                        physTrans = Matrix.Multiply(subShape.PositiveCenterMatrix, physTrans);

                        //must account collider shape scaling
                        Matrix worldTrans;
                        Matrix.Multiply(ref subShape.DebugPrimitiveScaling, ref physTrans, out worldTrans);

                        mPhysicsSystem.PhysicsEngine.DebugEffect.WorldViewProj = worldTrans * viewProj;
                        mPhysicsSystem.PhysicsEngine.DebugEffect.Color = element.Collider.IsActive ? Color.Green : Color.Red;

                        mPhysicsSystem.PhysicsEngine.DebugEffect.Apply();

                        subShape.DebugPrimitive.Draw();
                    }
                }
                else if (shape.Type != ColliderShapeTypes.StaticPlane) //a single shape
                {
                    var physTrans = element.BoneIndex == -1 ? element.Collider.PhysicsWorldTransform : element.BoneWorldMatrix;

                    //must account collider shape scaling
                    Matrix worldTrans;
                    Matrix.Multiply(ref element.Shape.Shape.DebugPrimitiveScaling, ref physTrans, out worldTrans);

                    mPhysicsSystem.PhysicsEngine.DebugEffect.WorldViewProj = worldTrans * viewProj;
                    mPhysicsSystem.PhysicsEngine.DebugEffect.Color = element.Collider.IsActive ? Color.Green : Color.Red;

                    mPhysicsSystem.PhysicsEngine.DebugEffect.Apply();

                    shape.DebugPrimitive.Draw();
                }
            }
        }

        public override void Update(GameTime time)
        {
            if (!mPhysicsSystem.PhysicsEngine.Initialized) return;

            //Simulation processing is from here
            mPhysicsSystem.PhysicsEngine.Update((float)time.Elapsed.TotalSeconds);

            //characters need manual updating
            foreach (var element in mCharacters.Where(element => element.Collider.Enabled))
            {
                element.UpdateTransformationComponent(element.Collider.PhysicsWorldTransform);
            }
        }

        //public override void Draw(GameTime time)
        //{
        //    if (!mPhysicsSystem.PhysicsEngine.Initialized) return;

        //    //process all enabled elements
        //    foreach (var e in mElementsToUpdateDraw)
        //    {
        //        var collider = e.Collider;

        //        var mesh = e.Data.ModelComponent;
        //        if (mesh == null) continue;

        //        var nodeTransform = mesh.ModelViewHierarchy.NodeTransformations[e.BoneIndex];

        //        Vector3 translation;
        //        Vector3 scale;
        //        Quaternion rotation;
        //        nodeTransform.WorldMatrix.Decompose(out scale, out rotation, out translation); //derive rot and translation, scale is ignored for now
        //        if (collider.UpdateTransformation(ref rotation, ref translation))
        //        {
        //            //true, Phys is the authority so we need to update the transformation
        //            TransformationComponent.CreateMatrixTRS(ref translation, ref rotation, ref scale, out nodeTransform.WorldMatrix);
        //            if (nodeTransform.ParentIndex != -1) //assuming -1 is root node
        //            {
        //                var parentWorld = mesh.ModelViewHierarchy.NodeTransformations[nodeTransform.ParentIndex];
        //                var inverseParent = parentWorld.WorldMatrix;
        //                inverseParent.Invert();
        //                nodeTransform.LocalMatrix = Matrix.Multiply(nodeTransform.WorldMatrix, inverseParent);
        //            }
        //            else
        //            {
        //                nodeTransform.LocalMatrix = nodeTransform.WorldMatrix;
        //            }
        //        }

        //        e.BoneWorldMatrix = Matrix.AffineTransformation(1.0f, rotation, translation);

        //        //update TRS
        //        nodeTransform.LocalMatrix.Decompose(out nodeTransform.Transform.Scaling, out nodeTransform.Transform.Rotation, out nodeTransform.Transform.Translation);

        //        mesh.ModelViewHierarchy.NodeTransformations[e.BoneIndex] = nodeTransform; //its a struct so we need to copy back
        //    }
        //}
    }
}

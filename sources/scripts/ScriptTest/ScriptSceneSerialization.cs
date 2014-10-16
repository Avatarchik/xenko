﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SiliconStudio.Paradox;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games.IO;
using SiliconStudio.Paradox.Games.MicroThreading;
using SiliconStudio.Paradox.Games.Serialization.Assets;
using SiliconStudio.Paradox.Games.Serialization.Contents;

using ScriptTest2;

namespace ScriptTest
{
    [ParadoxScript]
    public class ScriptSceneSerialization
    {
        public static string gitFolder;

        [ParadoxScript]
        public static void SetupFolder1(EngineContext engineContext)
        {
            gitFolder = "..\\..\\hotei_data1\\";
            VirtualFileSystem.MountFileSystem("/sync", gitFolder);
        }

        [ParadoxScript]
        public static void SetupFolder2(EngineContext engineContext)
        {
            gitFolder = "..\\..\\hotei_data2\\";
            VirtualFileSystem.MountFileSystem("/sync", gitFolder);
        }
        
        [ParadoxScript]
        public static async Task SyncSceneRebase(EngineContext engineContext)
        {
            // Save
            await SaveScene(engineContext);

            Process.Start(new ProcessStartInfo("git", "add package_scene.hotei") { WorkingDirectory = gitFolder, CreateNoWindow = true, UseShellExecute = false }).WaitForExit();
            Process.Start(new ProcessStartInfo("git", "commit -m Message") { WorkingDirectory = gitFolder, CreateNoWindow = true, UseShellExecute = false }).WaitForExit();
            Process.Start(new ProcessStartInfo("git", "fetch --all") { WorkingDirectory = gitFolder, CreateNoWindow = true, UseShellExecute = false }).WaitForExit();
            Process.Start(new ProcessStartInfo("git", "rebase origin/master") { WorkingDirectory = gitFolder, CreateNoWindow = true, UseShellExecute = false }).WaitForExit();
            Process.Start(new ProcessStartInfo("git", "push origin master") { WorkingDirectory = gitFolder, CreateNoWindow = true, UseShellExecute = false }).WaitForExit();

            // Load
            LoadScene(engineContext);
        }

        [ParadoxScript]
        public static async Task SyncSceneMerge(EngineContext engineContext)
        {
            // Save
            await SaveScene(engineContext);

            Process.Start(new ProcessStartInfo("git", "add package_scene.hotei") { WorkingDirectory = gitFolder, CreateNoWindow = true, UseShellExecute = false }).WaitForExit();
            Process.Start(new ProcessStartInfo("git", "commit -m Message") { WorkingDirectory = gitFolder, CreateNoWindow = true, UseShellExecute = false }).WaitForExit();
            Process.Start(new ProcessStartInfo("git", "fetch --all") { WorkingDirectory = gitFolder, CreateNoWindow = true, UseShellExecute = false }).WaitForExit();
            Process.Start(new ProcessStartInfo("git", "merge origin/master") { WorkingDirectory = gitFolder, CreateNoWindow = true, UseShellExecute = false }).WaitForExit();
            Process.Start(new ProcessStartInfo("git", "push origin master") { WorkingDirectory = gitFolder, CreateNoWindow = true, UseShellExecute = false }).WaitForExit();

            // Load
            LoadScene(engineContext);
        }
        
        [ParadoxScript]
        public static async Task SyncSceneLoad(EngineContext engineContext)
        {
            Process.Start(new ProcessStartInfo("git", "fetch --all") { WorkingDirectory = gitFolder, CreateNoWindow = true, UseShellExecute = false }).WaitForExit();
            Process.Start(new ProcessStartInfo("git", "pull") { WorkingDirectory = gitFolder, CreateNoWindow = true, UseShellExecute = false }).WaitForExit();

            // Load
            LoadScene(engineContext);
        }

        //[ParadoxScript(ScriptFlags.AssemblyStartup)]
        public static async Task SaveScene2(EngineContext engineContext)
        {
            var assetManager = new AssetManager(new AssetSerializerContextGenerator(engineContext.PackageManager));

            var entity = new Entity();
            var meshComponent = entity.GetOrCreate(ModelComponent.Key);
            meshComponent.SubMeshes.Add(new EffectMeshData { MeshData = new SubMeshData { DrawCount = 321 } });
            var entities = new[] { entity };

            throw new NotImplementedException();
            //var convertedEntities = assetManager.Convert<EntityGroup, IList<Entity>>(entities, "/data/package_scene.hotei#");
            //assetManager.Save(convertedEntities);

            //var contents = ParameterContainerExtensions.EnumerateContentData(convertedEntities).ToArray();
            //var sceneText = ParameterContainerExtensions.ConvertToText(contents[0]);
            //File.WriteAllText("current_scene.txt", sceneText);

            //ParameterContainerExtensions.ConvertFromText(engineContext, sceneText, "/data/package_scene_copy.hotei#/root");
            //var convertedEntities2 = assetManager.Load<EntityGroup>("/data/package_scene_copy.hotei#");
        }

        [ParadoxScript]
        public static async Task SaveScene(EngineContext engineContext)
        {
            var oldState = engineContext.EntityManager.State;
            engineContext.EntityManager.State = GameState.Saving;
            await Scheduler.Current.NextFrame();
            await Scheduler.Current.NextFrame();
            await Scheduler.Current.NextFrame();

            var contentManager = new AssetManager(new AssetSerializerContextGenerator(engineContext.PackageManager, ParameterContainerExtensions.DefaultSceneSerializerSelector));

            var entities = engineContext.EntityManager.Entities.ToArray();
            var sceneData = new EntityGroup { Entities = entities.ToList() };
            contentManager.Url.Set(sceneData, "/sync/package_scene.hotei#");
            contentManager.Save(sceneData);

            engineContext.EntityManager.State = GameState.Running;
        }

        [ParadoxScript]
        public static void ClearScene(EngineContext engineContext)
        {
            engineContext.EntityManager.GetSystem<HierarchicalProcessor>().RootEntities.Clear();
        }

        [ParadoxScript]
        public static async void LoadScene(EngineContext engineContext)
        {
            //var contentManager = new ContentManager(new ContentSerializerContextGenerator(VirtualFileStorage, engineContext.PackageManager, ParameterContainerExtensions.DefaultSceneSerializer));
            var contentManager = engineContext.AssetManager;

            engineContext.EntityManager.GetSystem<HierarchicalProcessor>().RootEntities.Clear();

            var sceneData = contentManager.Load<EntityGroup>("/sync/package_scene.hotei#");

            foreach (var entity in sceneData.Entities)
            {
                engineContext.EntityManager.AddEntity(entity);
                if (entity.ContainsKey(AnimationComponent.Key))
                {
                    var entityCopy = entity;
                    Scheduler.Current.Add(() => AnimScript.AnimateFBXModel(engineContext, entityCopy));
                }
            }
        }

        [ParadoxScript]
        public static async Task MergeSceneTest(EngineContext engineContext)
        {
            VirtualFileSystem.MountFileSystem("/global_data", "..\\..\\deps\\data\\");
            VirtualFileSystem.MountFileSystem("/global_data2", "..\\..\\data\\");
            ParameterContainerExtensions.Merge(engineContext.PackageManager, "/data/package_scene_base.hotei", "/data/package_scene1.hotei", "/data/package_scene2.hotei", "/data/package_scene_copy.hotei");
        }
    }
}
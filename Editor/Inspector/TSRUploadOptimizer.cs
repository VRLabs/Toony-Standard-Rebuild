using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem;

namespace VRLabs.ToonyStandardRebuild
{

    #if VRC_SDK_VRCSDK3
    public class LockMaterialsOnUpload : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => 70;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            Directory.CreateDirectory("Assets/VRLabs/GeneratedAssets/Shaders");
            Directory.CreateDirectory("Assets/VRLabs/GeneratedAssets/Materials");
            var _loadedShaders = FindAssetsByType<ModularShader>().ToList();
            var renderers = avatarGameObject.GetComponentsInChildren<Renderer>(true);
            var materialByGuid = new Dictionary<string, Material>();
            foreach (Renderer renderer in renderers)
            {
                var oldMats = renderer.sharedMaterials;
                var mats = new Material[oldMats.Length];
                for (int i = 0; i < oldMats.Length; i++)
                {
                    if (_loadedShaders.FirstOrDefault(y => y.LastGeneratedShaders.Contains(oldMats[i].shader)) != null)
                    {
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(oldMats[i], out string guid, out long _);
                        if (materialByGuid.TryGetValue(guid, out Material mat))
                        {
                            mats[i] = mat;
                        }
                        else
                        {
                            mats[i] = new Material(oldMats[i]);
                            AssetDatabase.CreateAsset(mats[i], $"Assets/VRLabs/GeneratedAssets/Materials/{guid}.mat"); 
                            materialByGuid.Add(guid, mats[i]);
                        }
                    }
                    else
                    {
                        mats[i] = oldMats[i];
                    }
                }
                renderer.materials = mats;
            }
            
            AssetDatabase.Refresh();
            
            var materials = renderers
                .SelectMany(r => r.sharedMaterials)
                .Distinct()
                .GroupBy(x => _loadedShaders.FirstOrDefault(y => y.LastGeneratedShaders.Contains(x.shader)));

            var contexts = new List<ShaderGenerator.ShaderContext>();
            
            foreach (IGrouping<ModularShader,Material> material in materials)
            {
                if(material.Key != null)
                    contexts.AddRange(ShaderGenerator.EnqueueShadersToGenerate("Assets/VRLabs/GeneratedAssets/Shaders", material.Key, material.AsEnumerable()));
            }
            contexts.GenerateMinimalShaders();
            //returning true all the time, because build process cant be stopped it seems
            return true;
        }
        
        private static T[] FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            AssetDatabase.Refresh();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).ToString().Replace("UnityEngine.", "")}");
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets.ToArray();
        }
    }
    #endif
}
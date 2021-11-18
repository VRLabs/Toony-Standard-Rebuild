#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem;

namespace VRLabs.ToonyStandardRebuild
{
    public static class TSRUploadOptimizer
    {
        private static Dictionary<ModularShader, Dictionary<string, Dictionary<string, int>>> _uvSetsPerMaterial;
        public class LockMaterialsOnUpload : IVRCSDKPreprocessAvatarCallback
        {
            public int callbackOrder => 70;

            public bool OnPreprocessAvatar(GameObject avatarGameObject)
            {
                Directory.CreateDirectory("Assets/VRLabs/GeneratedAssets/Shaders");
                Directory.CreateDirectory("Assets/VRLabs/GeneratedAssets/Materials");
                var loadedShaders = TSRUtilities.FindAssetsByType<ModularShader>().ToList();
                var renderers = avatarGameObject.GetComponentsInChildren<Renderer>(true);
                var materialByGuid = new Dictionary<string, Material>();
                foreach (Renderer renderer in renderers)
                    SwapMaterialsInRenderer(renderer, loadedShaders, materialByGuid);
                
                AssetDatabase.Refresh();
                
                GenerateMaterialsInRenderers(renderers, loadedShaders);
                return true;
            }

            private static void SwapMaterialsInRenderer(Renderer renderer, List<ModularShader> loadedShaders, Dictionary<string, Material> materialByGuid)
            {
                var oldMats = renderer.sharedMaterials;
                var mats = new Material[oldMats.Length];
                for (int i = 0; i < oldMats.Length; i++)
                {
                    if (loadedShaders.FirstOrDefault(y => y.LastGeneratedShaders.Contains(oldMats[i].shader)) != null)
                        SwapMaterial(materialByGuid, oldMats, i, mats);
                    else
                        mats[i] = oldMats[i];
                }

                renderer.materials = mats;
            }

            private static void SwapMaterial(Dictionary<string, Material> materialByGuid, Material[] oldMats, int i, Material[] mats)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(oldMats[i], out string guid, out long _);
                if (materialByGuid.TryGetValue(guid, out Material mat))
                {
                    mats[i] = mat;
                }
                else
                {
                    if (File.Exists($"Assets/VRLabs/GeneratedAssets/Materials/{guid}.mat"))
                    {
                        mats[i] = AssetDatabase.LoadAssetAtPath<Material>($"Assets/VRLabs/GeneratedAssets/Materials/{guid}.mat");
                        mats[i].CopyPropertiesFromMaterial(oldMats[i]);
                        mats[i].shader = oldMats[i].shader;
                    }
                    else
                    {
                        mats[i] = new Material(oldMats[i]);
                        AssetDatabase.CreateAsset(mats[i], $"Assets/VRLabs/GeneratedAssets/Materials/{guid}.mat");
                    }

                    materialByGuid.Add(guid, mats[i]);
                }
            }
        }

        public class LockMaterialsOnWorldUpload : IVRCSDKBuildRequestedCallback
        {
            public int callbackOrder => 70;

            bool IVRCSDKBuildRequestedCallback.OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
            {
                if (requestedBuildType != VRCSDKRequestedBuildType.Scene) return true;
                if (!(Object.FindObjectsOfType(typeof(VRC_SceneDescriptor)) is VRC_SceneDescriptor[] descriptors) || descriptors.Length <= 0) return true;
                
                var renderers = Object.FindObjectsOfType<Renderer>();
                var loadedShaders = TSRUtilities.FindAssetsByType<ModularShader>().ToList();
                GenerateMaterialsInRenderers(renderers, loadedShaders);

                return true;
            }
        }
        
        private static void GenerateMaterialsInRenderers(Renderer[] renderers, List<ModularShader> loadedShaders)
        {
            Directory.CreateDirectory("Assets/VRLabs/GeneratedAssets/Shaders");
            
            var materials = renderers
                .SelectMany(r => r.sharedMaterials)
                .Distinct()
                .GroupBy(x => loadedShaders.FirstOrDefault(y => y.LastGeneratedShaders.Contains(x.shader)));

            var contexts = new List<ShaderGenerator.ShaderContext>();

            _uvSetsPerMaterial = new Dictionary<ModularShader, Dictionary<string, Dictionary<string, int>>>();
            
            foreach (IGrouping<ModularShader, Material> material in materials)
            {
                if (material.Key == null) continue;
                if (_uvSetsPerMaterial.ContainsKey(material.Key)) continue;
                contexts.AddRange(ShaderGenerator.EnqueueShadersToGenerate("Assets/VRLabs/GeneratedAssets/Shaders", material.Key, material.AsEnumerable(), PostGeneration));
                   
                var uvSets = TSRUtilities.LoadUvSet(material.Key);
                _uvSetsPerMaterial.Add(material.Key, uvSets);
            }

            contexts.GenerateMinimalShaders();
            _uvSetsPerMaterial = null;
        }

        public static void PostGeneration(StringBuilder shaderFile, ShaderGenerator.ShaderContext context)
        {
            Dictionary<string, Dictionary<string, int>> uvSetsDictionary = _uvSetsPerMaterial[context.Shader];
            MatchCollection m = Regex.Matches(shaderFile.ToString(), @"#K#IDX#.*(?=])", RegexOptions.Multiline);

            for (int i = m.Count - 1; i >= 0; i--)
            {
                string uvSets = m[i].Value.Remove(0, 7);
                string[] pieces = uvSets.Split('#');

                if (pieces.Length != 2) continue;
                string uvSet = pieces[1];

                Dictionary<string, int> uvSetDictionary = uvSetsDictionary.TryGetValue(pieces[0], out Dictionary<string, int> res) ? res : null;
                if (uvSetDictionary == null) continue;
                if (!uvSetDictionary.TryGetValue(uvSet, out int value)) continue;
                
                shaderFile.Replace(m[i].Value, $"{value}");
            }
        }
    }
}
#endif
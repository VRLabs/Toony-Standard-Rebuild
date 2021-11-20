using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem;

namespace VRLabs.ToonyStandardRebuild
{
    public static class TSRUtilities
    {
        public static T[] FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            AssetDatabase.Refresh();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).ToString().Replace("UnityEngine.", "")}");
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                    assets.Add(asset);
            }
            return assets.ToArray();
        }
        
        public static Dictionary<string, Dictionary<string, int>> LoadUvSet(ModularShader shader)
        {
            var modules = new List<ModuleUI>();
            modules.Add(TSRGUI.LoadSerializedData(shader.AdditionalSerializedData));
            foreach (var shaderModule in ShaderGenerator.FindAllModules(shader))
                modules.Add(TSRGUI.LoadSerializedData(shaderModule.AdditionalSerializedData));
            
            var uvSets = new Dictionary<string, Dictionary<string, int>>();
            foreach (UVSet uvSet in modules.Where(module => module.UVSets != null).SelectMany(module => module.UVSets))
            {
                Dictionary<string, int> uvSetDictionary;
                if (uvSets.TryGetValue(uvSet.ID, out Dictionary<string, int> foundSet))
                {
                    uvSetDictionary = foundSet;
                }
                else
                {
                    uvSetDictionary = new Dictionary<string, int>();
                    uvSets.Add(uvSet.ID, uvSetDictionary);
                }

                foreach (UVItem uvItem in uvSet.Items)
                    if (!uvSetDictionary.ContainsKey(uvItem.ID))
                        uvSetDictionary.Add(uvItem.ID, uvSetDictionary.Count);
            }

            return uvSets;
        }
        
        public static void TSRPostGeneration(StringBuilder shaderFile, Dictionary<string, Dictionary<string, int>> sets)
        {
            MatchCollection m = Regex.Matches(shaderFile.ToString(), @"#K#IDX#.*(?=])", RegexOptions.Multiline);

            for (int i = m.Count - 1; i >= 0; i--)
            {
                string uvSets = m[i].Value.Remove(0, 7);
                string[] pieces = uvSets.Split('#');

                if (pieces.Length != 2) continue;
                string uvSet = pieces[1];

                Dictionary<string, int> uvSetDictionary = sets.TryGetValue(pieces[0], out Dictionary<string, int> res) ? res : null;
                if (uvSetDictionary == null) continue;
                if (!uvSetDictionary.TryGetValue(uvSet, out int value)) continue;

                shaderFile.Replace(m[i].Value, $"{value}");
            }
        }
    }
}
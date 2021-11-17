using System.Collections.Generic;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public static class MaterialArrayHelper
    {
        public static void SetKeyword(this IEnumerable<Material> materials, string keyword, bool state)
        {
            foreach (var m in materials)
            {
                if (state)
                    m.EnableKeyword(keyword);
                else
                    m.DisableKeyword(keyword);
            }
        }

        public static bool IsKeywordMixedValue(this Material[] materials, string keyword)
        {
            bool reference = materials[0].IsKeywordEnabled(keyword);
            foreach (var m in materials)
                if (m.IsKeywordEnabled(keyword) != reference)
                    return true;
            return false;
        }

        public static void SetOverrideTag(this IEnumerable<Material> materials, string tagName, string value)
        {
            foreach (var m in materials)
                m.SetOverrideTag(tagName, value);
        }

        public static void SetInt(this IEnumerable<Material> materials, string name, int value)
        {
            foreach (var m in materials)
                m.SetInt(name, value);
        }
        
        public static void SetFloat(this IEnumerable<Material> materials, string name, float value)
        {
            foreach (var m in materials)
                m.SetFloat(name, value);
        }

        public static void SetVector(this IEnumerable<Material> materials, string name, Vector4 value)
        {
            foreach (var m in materials)
                m.SetVector(name, value);
        }
        
        public static void SetColor(this IEnumerable<Material> materials, string name, Color value)
        {
            foreach (var m in materials)
                m.SetColor(name, value);
        }
        
        public static void SetTexture(this IEnumerable<Material> materials, string name, Texture value)
        {
            foreach (var m in materials)
                m.SetTexture(name, value);
        }

        public static void SetRenderQueue(this IEnumerable<Material> materials, int queue)
        {
            foreach (var m in materials)
                m.renderQueue = queue;
        }
    }
}
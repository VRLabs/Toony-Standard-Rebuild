using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRLabs.ToonyStandardRebuild.SimpleShaderInspectors;

namespace VRLabs.ToonyStandardRebuild
{
    public class UpdateData
    {
        public List<UpdateProp<float>> FloatProperties;
        public List<UpdateProp<Color>> ColorProperties;
        public List<UpdateProp<Texture>> TextureProperties;
        public List<UpdateProp<bool>> Keywords;
        public List<UpdateProp<string>> OverrideTags;

        public int RenderQueue;

        public UpdateData()
        {
            FloatProperties = new List<UpdateProp<float>>();
            ColorProperties = new List<UpdateProp<Color>>();
            TextureProperties = new List<UpdateProp<Texture>>();
            Keywords = new List<UpdateProp<bool>>();
            OverrideTags = new List<UpdateProp<string>>();

            RenderQueue = -1;
        }

        public void UpdateMaterials(IEnumerable<Material> materials)
        {
            IEnumerable<Material> enumerable = materials as Material[] ?? materials.ToArray();
            foreach (var value in FloatProperties)
                enumerable.SetFloat(value.Name, value.Value);
            foreach (var value in ColorProperties)
                enumerable.SetColor(value.Name, value.Value);
            foreach (var value in TextureProperties)
                enumerable.SetTexture(value.Name, value.Value);
            foreach (var value in Keywords)
                enumerable.SetKeyword(value.Name, value.Value);
            foreach (var value in OverrideTags)
                enumerable.SetOverrideTag(value.Name, value.Value);
            
            enumerable.SetRenderQueue(RenderQueue);
        }
    }
}
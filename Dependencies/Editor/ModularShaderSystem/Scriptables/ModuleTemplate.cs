using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace VRLabs.ToonyStandardRebuild.ModularShaderSystem
{
    [Serializable]
    public class ModuleTemplate 
    {
        public TemplateAsset Template;
        [FormerlySerializedAs("Keyword")] public List<string> Keywords;
        [FormerlySerializedAs("IsCGOnly")] public bool NeedsVariant;
        public int Queue = 100;
        public List<string> TemplateKeywords;
    }
}
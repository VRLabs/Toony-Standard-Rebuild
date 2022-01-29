using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace VRLabs.ToonyStandardRebuild.ModularShaderSystem.UI
{
    [CustomEditor(typeof(ShaderModule))]
    public class ShaderModuleEditor : Editor
    {
        private VisualElement _root;

        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();

            var visualTree = Resources.Load<VisualTreeAsset>(MSSConstants.RESOURCES_FOLDER + "/MSSUIElements/ShaderModuleEditor");
            VisualElement template = visualTree.CloneTree();
            _root.Add(template);

            return _root;
        }
    }
}
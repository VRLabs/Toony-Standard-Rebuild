using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class SpaceControl : SimpleControl
    {
        public int Space { get; set; }

        public SpaceControl(int space = 0) : base("")
        {
            Space = space;
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            if (Space == 0)
                EditorGUILayout.Space();
            else
                GUILayout.Space(Space);
        }
    }
}
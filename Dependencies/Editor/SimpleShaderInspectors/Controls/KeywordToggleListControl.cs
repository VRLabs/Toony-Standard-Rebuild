using System.Collections.Generic;
using UnityEditor;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class KeywordToggleListControl : KeywordToggleControl, IControlContainer
    {
        public List<SimpleControl> Controls { get; set; }

        public KeywordToggleListControl(string keyword) : base(keyword)
        {
            Controls = new List<SimpleControl>();
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            base.ControlGUI(materialEditor);
            if (ToggleEnabled)
            {
                EditorGUI.indentLevel++;
                foreach (var control in Controls)
                    control.DrawControl(materialEditor);
                
                EditorGUI.indentLevel--;
            }
        }

        public void AddControl(SimpleControl control, string alias = "") => Controls.AddControl(control, alias);

        public IEnumerable<SimpleControl> GetControlList() => Controls;
    }
}
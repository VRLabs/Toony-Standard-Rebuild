using System.Collections.Generic;
using UnityEditor;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class ToggleListControl : ToggleControl, IControlContainer
    {
        public List<SimpleControl> Controls { get; set; }

        public ToggleListControl(string propertyName, float falseValue = 0, float trueValue = 1) : base(propertyName, falseValue, trueValue)
        {
            Controls = new List<SimpleControl>();
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            base.ControlGUI(materialEditor);
            if (ToggleEnabled)
            {
                EditorGUI.indentLevel++;
                foreach (SimpleControl control in Controls)
                {
                    control.DrawControl(materialEditor);
                }
                EditorGUI.indentLevel--;
            }
        }
        
        public void AddControl(SimpleControl control) => Controls.Add(control);

        public IEnumerable<SimpleControl> GetControlList() => Controls;
    }
}
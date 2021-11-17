using System.Collections.Generic;
using UnityEditor;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class ControlContainer : SimpleControl, IControlContainer
    {
        public List<SimpleControl> Controls { get; set; }

        public ControlContainer() : base("")
        {
            Controls = new List<SimpleControl>();
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            foreach (var control in Controls)
                control.DrawControl(materialEditor);
        }
        
        public void AddControl(SimpleControl control)
        {
            Controls.Add(control);
        }
        
        public IEnumerable<SimpleControl> GetControlList()
        {
            return Controls;
        }
    }
}
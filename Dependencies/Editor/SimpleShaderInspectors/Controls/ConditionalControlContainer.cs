using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class ConditionalControlContainer : PropertyControl, IControlContainer
    {
        public List<SimpleControl> Controls { get; set; }
        
        [Chainable]
        public bool Indent { get; set; }
        
        protected readonly float EnableValue;

        public ConditionalControlContainer(string conditionalProperty, float enableValue) : base(conditionalProperty)
        {
            EnableValue = enableValue;
            Controls = new List<SimpleControl>();
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            if (!(Math.Abs(Property.floatValue - EnableValue) < 0.001)) return;
            if(Indent) EditorGUI.indentLevel++;
            foreach (var control in Controls)
                control.DrawControl(materialEditor);
            if (Indent) EditorGUI.indentLevel--;
        }
        
        public void AddControl(SimpleControl control, string alias = "") => Controls.AddControl(control, alias);

        public IEnumerable<SimpleControl> GetControlList() => Controls;
    }
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public class HorizontalContainer : SimpleControl, IControlContainer
    {
        private float _windowWidth;
        private float _width;

        public List<SimpleControl> Controls { get; set; }

        public HorizontalContainer() : base("")
        {
            Controls = new List<SimpleControl>();
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            if(Event.current.type != EventType.Repaint)
            {
                float oldWindowWidth = _windowWidth;
                _windowWidth = EditorGUIUtility.currentViewWidth;
                float difference = _windowWidth - oldWindowWidth;
                _width += difference;
            }
            
            EditorGUILayout.BeginHorizontal();
            float elementWidth = Controls.Count > 0 ? _width / Controls.Count : _width;
            float oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = Controls.Count > 0 ? EditorGUIUtility.labelWidth / Controls.Count : EditorGUIUtility.labelWidth;
            foreach (var control in Controls)
            {
                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(elementWidth));
                control.DrawControl(materialEditor);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUIUtility.labelWidth = oldWidth;
            
            if (Event.current.type == EventType.Repaint)
            {
                _windowWidth = EditorGUIUtility.currentViewWidth;
                _width = GUILayoutUtility.GetLastRect().width;
            }
        }
        
        public void AddControl(SimpleControl control, string alias = "") => Controls.AddControl(control, alias);

        public IEnumerable<SimpleControl> GetControlList() => Controls;
    }
}
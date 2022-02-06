using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public abstract class SimpleControl
    {
        public virtual ISimpleShaderInspector Inspector { get; set; }
        public GUIContent Content { get; set; }

        public string ControlAlias { get; set; }

        public bool IsVisible { get; set; }

        public bool IsEnabled { get; set; }

        protected SimpleControl(string alias)
        {
            ControlAlias = alias;
            IsEnabled = true;
            IsVisible = true;
        }

        protected abstract void ControlGUI(MaterialEditor materialEditor);

        public void DrawControl(MaterialEditor materialEditor)
        {
            if (!IsVisible) return;
            if (!IsEnabled)
            {
                EditorGUI.BeginDisabledGroup(true);
                ControlGUI(materialEditor);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                ControlGUI(materialEditor);
            }
        }
    }

    public static partial class Extensions
    {
        public static T WithAlias<T>(this T control, string alias) where T : SimpleControl
        {
            control.ControlAlias = alias;
            return control;
        }

        public static T WithVisibility<T>(this T control, bool visible) where T : SimpleControl
        {
            control.IsVisible = visible;
            return control;
        }

        public static T WithEnabled<T>(this T control, bool enabled) where T : SimpleControl
        {
            control.IsEnabled = enabled;
            return control;
        }
    }
}
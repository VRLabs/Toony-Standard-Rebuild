using UnityEditor;
using UnityEngine.UIElements;
using VRLabs.ToonyStandardRebuild.ModularShaderSystem.Debug;

namespace VRLabs.ToonyStandardRebuild.ModularShaderSystem.UI
{
    public class TextPopup : EditorWindow
    {
        public string Text;
        private void CreateGUI()
        {
            var viewer = new CodeViewElement();
            viewer.Text = Text;
            viewer.StretchToParentSize();
            var darkThemeStyleSheet = EditorGUIUtility.Load("StyleSheets/Generated/DefaultCommonDark_inter.uss.asset") as StyleSheet;
            rootVisualElement.styleSheets.Add(darkThemeStyleSheet);
            rootVisualElement.Add(viewer);
        }
    }
}
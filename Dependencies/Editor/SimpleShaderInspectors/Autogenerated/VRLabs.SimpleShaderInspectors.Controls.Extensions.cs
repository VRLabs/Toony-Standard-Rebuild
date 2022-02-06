namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls
{
    public static partial class Chainables
    {
        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.ColorControl AddColorControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, System.Boolean showAlphaValue = true, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.ColorControl(propertyName, showAlphaValue);
            container.AddControl(control, appendAfterAlias);
            return control;
        }
        public static T WithShowAlphaValue<T>(this T control, System.Boolean property) where T : ColorControl
        {
            control.ShowAlphaValue = property;
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.ConditionalControlContainer AddConditionalControlContainer(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String conditionalProperty, System.Single enableValue, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.ConditionalControlContainer(conditionalProperty, enableValue);
            container.AddControl(control, appendAfterAlias);
            return control;
        }
        public static T WithIndent<T>(this T control, System.Boolean property) where T : ConditionalControlContainer
        {
            control.Indent = property;
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.ControlContainer AddControlContainer(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.ControlContainer();
            container.AddControl(control, appendAfterAlias);
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.EnumControl<TEnum> AddEnumControl<TEnum>(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, string appendAfterAlias = "") where TEnum : System.Enum
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.EnumControl<TEnum>(propertyName);
            container.AddControl(control, appendAfterAlias);
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.GradientTextureControl AddGradientTextureControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, System.String colorPropertyName = null, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.GradientTextureControl(propertyName, colorPropertyName);
            container.AddControl(control, appendAfterAlias);
            return control;
        }
        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.GradientTextureControl AddGradientTextureControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, System.String minColorPropertyName, System.String maxColorPropertyName, System.String colorPropertyName = null, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.GradientTextureControl(propertyName, minColorPropertyName, maxColorPropertyName, colorPropertyName);
            container.AddControl(control, appendAfterAlias);
            return control;
        }
        public static T WithGradientButtonStyle<T>(this T control, UnityEngine.GUIStyle property) where T : GradientTextureControl
        {
            control.GradientButtonStyle = property;
            return control;
        }
        public static T WithGradientSaveButtonStyle<T>(this T control, UnityEngine.GUIStyle property) where T : GradientTextureControl
        {
            control.GradientSaveButtonStyle = property;
            return control;
        }
        public static T WithGradientEditorStyle<T>(this T control, UnityEngine.GUIStyle property) where T : GradientTextureControl
        {
            control.GradientEditorStyle = property;
            return control;
        }
        public static T WithGradientButtonColor<T>(this T control, UnityEngine.Color property) where T : GradientTextureControl
        {
            control.GradientButtonColor = property;
            return control;
        }
        public static T WithGradientSaveButtonColor<T>(this T control, UnityEngine.Color property) where T : GradientTextureControl
        {
            control.GradientSaveButtonColor = property;
            return control;
        }
        public static T WithGradientEditorColor<T>(this T control, UnityEngine.Color property) where T : GradientTextureControl
        {
            control.GradientEditorColor = property;
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.HelpBoxControl AddHelpBoxControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String alias, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.HelpBoxControl(alias);
            container.AddControl(control, appendAfterAlias);
            return control;
        }
        public static T WithBoxType<T>(this T control, UnityEditor.MessageType property) where T : HelpBoxControl
        {
            control.BoxType = property;
            return control;
        }
        public static T WithIsWideBox<T>(this T control, System.Boolean property) where T : HelpBoxControl
        {
            control.IsWideBox = property;
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.HorizontalContainer AddHorizontalContainer(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.HorizontalContainer();
            container.AddControl(control, appendAfterAlias);
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.KeywordToggleControl AddKeywordToggleControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String keyword, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.KeywordToggleControl(keyword);
            container.AddControl(control, appendAfterAlias);
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.KeywordToggleListControl AddKeywordToggleListControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String keyword, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.KeywordToggleListControl(keyword);
            container.AddControl(control, appendAfterAlias);
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.LabelControl AddLabelControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String alias, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.LabelControl(alias);
            container.AddControl(control, appendAfterAlias);
            return control;
        }
        public static T WithLabelStyle<T>(this T control, UnityEngine.GUIStyle property) where T : LabelControl
        {
            control.LabelStyle = property;
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.LightmapEmissionControl AddLightmapEmissionControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.LightmapEmissionControl();
            container.AddControl(control, appendAfterAlias);
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.RGBASelectorControl AddRGBASelectorControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.RGBASelectorControl(propertyName);
            container.AddControl(control, appendAfterAlias);
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.SpaceControl AddSpaceControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.Int32 space = 0, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.SpaceControl(space);
            container.AddControl(control, appendAfterAlias);
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.TextureControl AddTextureControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, System.String extraPropertyName1 = null, System.String extraPropertyName2 = null, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.TextureControl(propertyName, extraPropertyName1, extraPropertyName2);
            container.AddControl(control, appendAfterAlias);
            return control;
        }
        public static T WithShowTilingAndOffset<T>(this T control, System.Boolean property) where T : TextureControl
        {
            control.ShowTilingAndOffset = property;
            return control;
        }
        public static T WithHasHDRColor<T>(this T control, System.Boolean property) where T : TextureControl
        {
            control.HasHDRColor = property;
            return control;
        }
        public static T WithOptionsButtonStyle<T>(this T control, UnityEngine.GUIStyle property) where T : TextureControl
        {
            control.OptionsButtonStyle = property;
            return control;
        }
        public static T WithOptionsAreaStyle<T>(this T control, UnityEngine.GUIStyle property) where T : TextureControl
        {
            control.OptionsAreaStyle = property;
            return control;
        }
        public static T WithOptionsButtonColor<T>(this T control, UnityEngine.Color property) where T : TextureControl
        {
            control.OptionsButtonColor = property;
            return control;
        }
        public static T WithOptionsAreaColor<T>(this T control, UnityEngine.Color property) where T : TextureControl
        {
            control.OptionsAreaColor = property;
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.TextureGeneratorControl AddTextureGeneratorControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, System.String extraPropertyName1 = null, System.String extraPropertyName2 = null, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.TextureGeneratorControl(propertyName, extraPropertyName1, extraPropertyName2);
            container.AddControl(control, appendAfterAlias);
            return control;
        }
        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.TextureGeneratorControl AddTextureGeneratorControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, UnityEngine.Shader shader, System.String propertyName, System.String extraPropertyName1 = null, System.String extraPropertyName2 = null, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.TextureGeneratorControl(shader, propertyName, extraPropertyName1, extraPropertyName2);
            container.AddControl(control, appendAfterAlias);
            return control;
        }
        public static T WithGeneratorButtonStyle<T>(this T control, UnityEngine.GUIStyle property) where T : TextureGeneratorControl
        {
            control.GeneratorButtonStyle = property;
            return control;
        }
        public static T WithGeneratorSaveButtonStyle<T>(this T control, UnityEngine.GUIStyle property) where T : TextureGeneratorControl
        {
            control.GeneratorSaveButtonStyle = property;
            return control;
        }
        public static T WithGeneratorCloseButtonStyle<T>(this T control, UnityEngine.GUIStyle property) where T : TextureGeneratorControl
        {
            control.GeneratorCloseButtonStyle = property;
            return control;
        }
        public static T WithGeneratorStyle<T>(this T control, UnityEngine.GUIStyle property) where T : TextureGeneratorControl
        {
            control.GeneratorStyle = property;
            return control;
        }
        public static T WithGeneratorInputStyle<T>(this T control, UnityEngine.GUIStyle property) where T : TextureGeneratorControl
        {
            control.GeneratorInputStyle = property;
            return control;
        }
        public static T WithGeneratorButtonColor<T>(this T control, UnityEngine.Color property) where T : TextureGeneratorControl
        {
            control.GeneratorButtonColor = property;
            return control;
        }
        public static T WithGeneratorSaveButtonColor<T>(this T control, UnityEngine.Color property) where T : TextureGeneratorControl
        {
            control.GeneratorSaveButtonColor = property;
            return control;
        }
        public static T WithGeneratorCloseButtonColor<T>(this T control, UnityEngine.Color property) where T : TextureGeneratorControl
        {
            control.GeneratorCloseButtonColor = property;
            return control;
        }
        public static T WithGeneratorColor<T>(this T control, UnityEngine.Color property) where T : TextureGeneratorControl
        {
            control.GeneratorColor = property;
            return control;
        }
        public static T WithGeneratorInputColor<T>(this T control, UnityEngine.Color property) where T : TextureGeneratorControl
        {
            control.GeneratorInputColor = property;
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.TilingAndOffsetControl AddTilingAndOffsetControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.TilingAndOffsetControl(propertyName);
            container.AddControl(control, appendAfterAlias);
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.ToggleControl AddToggleControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, System.Single falseValue = 0, System.Single trueValue = 1, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.ToggleControl(propertyName, falseValue, trueValue);
            container.AddControl(control, appendAfterAlias);
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.ToggleListControl AddToggleListControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, System.Single falseValue = 0, System.Single trueValue = 1, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.ToggleListControl(propertyName, falseValue, trueValue);
            container.AddControl(control, appendAfterAlias);
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.VectorControl AddVectorControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String propertyName, System.Boolean isXVisible = true, System.Boolean isYVisible = true, System.Boolean isZVisible = true, System.Boolean isWVisible = true, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.VectorControl(propertyName, isXVisible, isYVisible, isZVisible, isWVisible);
            container.AddControl(control, appendAfterAlias);
            return control;
        }

        public static VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.VertexStreamsControl AddVertexStreamsControl(this VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.IControlContainer container, System.String alias, string appendAfterAlias = "")
        {
            var control = new VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.VertexStreamsControl(alias);
            container.AddControl(control, appendAfterAlias);
            return control;
        }

    }
}
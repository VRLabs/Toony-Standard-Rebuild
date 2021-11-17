using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public static class Textures
    {
        private static Texture2D _colorIconBorder;
        public static Texture2D ColorIconBorder
        {
            get
            {
                if (_colorIconBorder != null) return _colorIconBorder;
                _colorIconBorder = Resources.Load<Texture2D>($"{SSIConstants.RESOURCES_FOLDER}/Textures/ColorIconBorder");
                return _colorIconBorder;
            }
        }

        private static Texture2D _colorIconBorderSelected;
        public static Texture2D ColorIconBorderSelected
        {
            get
            {
                if (_colorIconBorderSelected != null) return _colorIconBorderSelected;
                _colorIconBorderSelected = Resources.Load<Texture2D>($"{SSIConstants.RESOURCES_FOLDER}/Textures/ColorIconBorderSelected");
                return _colorIconBorderSelected;
            }
        }

        private static Texture2D _colorIcon;
        public static Texture2D ColorIcon
        {
            get
            {
                if (_colorIcon != null) return _colorIcon;
                _colorIcon = Resources.Load<Texture2D>($"{SSIConstants.RESOURCES_FOLDER}/Textures/ColorIcon");
                return _colorIcon;
            }
        }
    }

    public static class ComputeShaders
    {
        private static ComputeShader _rgbaPacker;
        public static ComputeShader RGBAPacker
        {
            get
            {
                if (_rgbaPacker != null) return _rgbaPacker;
                _rgbaPacker = Resources.Load<ComputeShader>($"{SSIConstants.RESOURCES_FOLDER}/ComputeShaders/RGBAPacker");
                return _rgbaPacker;
            }
        }
        public static string RGBAPackerSettings => Resources.Load<TextAsset>($"{SSIConstants.RESOURCES_FOLDER}/ComputeShaderSettings/RGBAPackerDefault").text;
    }

    public static class Styles
    {
        private static GUIStyle _bubble;
        public static GUIStyle Bubble
        {
            get
            {
                if (_bubble != null) return _bubble;
                _bubble = new GUIStyle("button");
                return _bubble;
            }
        }

        private static GUIStyle _box;

        public static GUIStyle Box
        {
            get
            {
                if (_box != null) return _box;
                _box = new GUIStyle("box");
                return _box;
            }
        }

        private static GUIStyle _textureBoxLightBorder;
        public static GUIStyle TextureBoxLightBorder
        {
            get
            {
                if (_textureBoxLightBorder != null) return _textureBoxLightBorder;
                _textureBoxLightBorder = CreateStyleFromSprite(new RectOffset(4, 4, 11, 4), $"{SSIConstants.RESOURCES_FOLDER}/Textures/TextureBoxLight");
                return _textureBoxLightBorder;
            }
        }

        private static GUIStyle _textureBoxHeavyBorder;
        public static GUIStyle TextureBoxHeavyBorder
        {
            get
            {
                if (_textureBoxHeavyBorder != null) return _textureBoxHeavyBorder;
                _textureBoxHeavyBorder = CreateStyleFromSprite(new RectOffset(4, 4, 11, 4), $"{SSIConstants.RESOURCES_FOLDER}/Textures/TextureBoxHeavy");
                return _textureBoxHeavyBorder;
            }
        }

        private static GUIStyle _boxLightBorder;
        public static GUIStyle BoxLightBorder
        {
            get
            {
                if (_boxLightBorder != null) return _boxLightBorder;
                _boxLightBorder = CreateStyleFromSprite(new RectOffset(4, 4, 4, 4), $"{SSIConstants.RESOURCES_FOLDER}/Textures/BoxLight");
                _boxLightBorder.alignment = TextAnchor.MiddleCenter;
                _boxLightBorder.normal.textColor = EditorStyles.label.normal.textColor;
                _boxLightBorder.active.textColor = EditorStyles.label.active.textColor;
                _boxLightBorder.hover.textColor = EditorStyles.label.hover.textColor;
                _boxLightBorder.focused.textColor = EditorStyles.label.focused.textColor;
                return _boxLightBorder;
            }
        }

        private static GUIStyle _boxHeavyBorder;
        public static GUIStyle BoxHeavyBorder
        {
            get
            {
                if (_boxHeavyBorder != null) return _boxHeavyBorder;
                _boxHeavyBorder = CreateStyleFromSprite(new RectOffset(4, 4, 4, 4), $"{SSIConstants.RESOURCES_FOLDER}/Textures/BoxHeavy");
                _boxHeavyBorder.alignment = TextAnchor.MiddleCenter;
                _boxHeavyBorder.normal.textColor = EditorStyles.label.normal.textColor;
                _boxHeavyBorder.active.textColor = EditorStyles.label.active.textColor;
                _boxHeavyBorder.hover.textColor = EditorStyles.label.hover.textColor;
                _boxHeavyBorder.focused.textColor = EditorStyles.label.focused.textColor;
                return _boxHeavyBorder;
            }
        }

        private static Texture2D _ssiLogoLight;
        public static Texture2D SSILogoLight
        {
            get
            {
                if (_ssiLogoLight != null) return _ssiLogoLight;

                _ssiLogoLight = Resources.Load<Texture2D>($"{SSIConstants.RESOURCES_FOLDER}/Textures/Logo/LogoLight");
                return _ssiLogoLight;

            }
        }

        private static Texture2D _ssiLogoDark;
        public static Texture2D SSILogoDark
        {
            get
            {
                if (_ssiLogoDark != null) return _ssiLogoDark;

                _ssiLogoDark = Resources.Load<Texture2D>($"{SSIConstants.RESOURCES_FOLDER}/Textures/Logo/LogoDark");
                return _ssiLogoDark;
            }
        }

        private static GUIStyle _deleteIcon;
        public static GUIStyle DeleteIcon
        {
            get
            {
                if (_deleteIcon != null) return _deleteIcon;
                _deleteIcon = CreateStyleFromSprite(normal: $"{SSIConstants.RESOURCES_FOLDER}/Textures/DeleteIcon",
                                                    active: $"{SSIConstants.RESOURCES_FOLDER}/Textures/DeleteIconPressed",
                                                     hover: $"{SSIConstants.RESOURCES_FOLDER}/Textures/DeleteIconHover"); //new GUIStyle("WinBtnClose");
                return _deleteIcon;
            }
        }

        private static GUIStyle _upIcon;

        public static GUIStyle UpIcon
        {
            get
            {
                if (_upIcon != null) return _upIcon;
                _upIcon = CreateStyleFromSprite(normal: $"{SSIConstants.RESOURCES_FOLDER}/Textures/UpIcon",
                                                active: $"{SSIConstants.RESOURCES_FOLDER}/Textures/UpIconPressed",
                                                 hover: $"{SSIConstants.RESOURCES_FOLDER}/Textures/UpIconHover"); //new GUIStyle("ProfilerTimelineRollUpArrow");
                return _upIcon;
            }
        }

        private static GUIStyle _downIcon;

        public static GUIStyle DownIcon
        {
            get
            {
                if (_downIcon != null) return _downIcon;
                _downIcon = CreateStyleFromSprite(normal: $"{SSIConstants.RESOURCES_FOLDER}/Textures/DownIcon",
                                                  active: $"{SSIConstants.RESOURCES_FOLDER}/Textures/DownIconPressed",
                                                   hover: $"{SSIConstants.RESOURCES_FOLDER}/Textures/DownIconHover"); //new GUIStyle("ProfilerTimelineDigDownArrow");
                return _downIcon;
            }
        }

        private static GUIStyle _gearIcon;

        public static GUIStyle GearIcon
        {
            get
            {
                if (_gearIcon != null) return _gearIcon;
                _gearIcon = CreateStyleFromSprite(normal: $"{SSIConstants.RESOURCES_FOLDER}/Textures/GearIcon",
                                                  active: $"{SSIConstants.RESOURCES_FOLDER}/Textures/GearIconPressed",
                                                   hover: $"{SSIConstants.RESOURCES_FOLDER}/Textures/GearIconHover");
                return _gearIcon;
            }
        } 
 
        private static GUIStyle _boldCenter;
        public static GUIStyle BoldCenter
        {
            get
            {
                if (_boldCenter != null) return _boldCenter;
                _boldCenter = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
                return _boldCenter;
            }
        }

        private static GUIStyle _boldLeft;
        public static GUIStyle BoldLeft
        {
            get
            {
                if (_boldLeft != null) return _boldLeft;
                _boldLeft = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft };
                return _boldLeft;
            }
        }

        private static GUIStyle _centerLabel;
        public static GUIStyle CenterLabel
        {
            get
            {
                if (_centerLabel != null) return _centerLabel;
                _centerLabel = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
                return _centerLabel;
            }
        }
        
        private static GUIStyle _bottomCenterLabel;
        public static GUIStyle BottomCenterLabel
        {
            get
            {
                if (_bottomCenterLabel != null) return _bottomCenterLabel;
                _bottomCenterLabel = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.LowerCenter };
                return _bottomCenterLabel;
            }
        }

        private static GUIStyle _rightLabel;
        public static GUIStyle RightLabel
        {
            get
            {
                if (_rightLabel != null) return _rightLabel;
                _rightLabel = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight };
                return _rightLabel;
            }
        }

        private static GUIStyle _multilineLabel;
        public static GUIStyle MultilineLabel
        {
            get
            {
                if (_multilineLabel != null) return _multilineLabel;
                _multilineLabel = new GUIStyle(EditorStyles.label)
                {
                    wordWrap = true
                };
                return _multilineLabel;
            }
        }

        public static GUIStyle CreateStyleFromSprite(string normal, string active = null, string focused = null, string hover = null)
        {
            return CreateStyleFromSprite(new RectOffset(4, 4, 4, 4), normal, active, focused, hover);
        }
        public static GUIStyle CreateStyleFromSprite(RectOffset padding, string normal, string active = null, string focused = null, string hover = null)
        {
            var style = new GUIStyle();
            Sprite sprite = Resources.Load<Sprite>(normal);
            style.padding = padding;
            if (sprite != null)
            {
                style.border.left = (int)sprite.border.x;
                style.border.bottom = (int)sprite.border.y;
                style.border.right = (int)sprite.border.z;
                style.border.top = (int)sprite.border.w;
            }

            if (!string.IsNullOrEmpty(normal)) style.normal.background = Resources.Load<Texture2D>(normal);
            if (!string.IsNullOrEmpty(active)) style.active.background = Resources.Load<Texture2D>(active);
            if (!string.IsNullOrEmpty(focused)) style.focused.background = Resources.Load<Texture2D>(focused);
            if (!string.IsNullOrEmpty(hover)) style.hover.background = Resources.Load<Texture2D>(hover);

            return style;
        }
    }
}
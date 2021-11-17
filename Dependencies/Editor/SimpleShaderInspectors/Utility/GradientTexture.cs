using System;
using UnityEngine;
using System.Collections.Generic;
namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Utility
{
    public enum GradientBlendMode
    {
        Linear,
        Fixed
    }

    public class GradientTexture
    {
        public List<ColorKey> Keys = new List<ColorKey>();

        public GradientBlendMode BlendMode;

        private Texture2D _texture;

        public GradientTexture(int width)
        {
            BlendMode = GradientBlendMode.Linear;
            Keys.Add(new ColorKey(Color.black, 0));
            Keys.Add(new ColorKey(Color.white, 1));

            _texture = new Texture2D(width, 1, TextureFormat.RGB24, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
            UpdateTexture();
        }

        public Color Evaluate(float time)
        {
            ColorKey keyLeft = Keys[0];
            ColorKey keyRight = Keys[Keys.Count - 1];

            for (int i = 0; i < Keys.Count; i++)
            {
                if (Keys[i].Time <= time)
                {
                    keyLeft = Keys[i];
                }
                if (Keys[i].Time >= time)
                {
                    keyRight = Keys[i];
                    break;
                }
            }

            if (BlendMode == GradientBlendMode.Fixed) return keyRight.Color;
            
            float blendTime = Mathf.InverseLerp(keyLeft.Time, keyRight.Time, time);
            return Color.Lerp(keyLeft.Color, keyRight.Color, blendTime);

        }

        public int AddKey(Color color, float time)
        {
            return AddKey(color, time, true);
        }
        private int AddKey(Color color, float time, bool shouldDelete)
        {
            for (int i = 0; i < Keys.Count; i++)
            {
                if (time < Keys[i].Time)
                {
                    Keys.Insert(i, new ColorKey(color, time));
                    UpdateTexture();
                    return i;
                }

                if (Math.Abs(time - Keys[i].Time) < 0.001 && shouldDelete)
                {
                    Keys[i] = new ColorKey(color, time);
                    UpdateTexture();
                    return -1;
                }
            }
            Keys.Add(new ColorKey(color, time));
            UpdateTexture();
            return Keys.Count - 1;
        }

        public void RemoveKey(int index)
        {
            RemoveKey(index, true);
        }

        private void RemoveKey(int index, bool checkMin)
        {
            if (Keys.Count > 1 && checkMin)
            {
                Keys.RemoveAt(index);
            }
            else if (!checkMin)
            {
                Keys.RemoveAt(index);
            }
            UpdateTexture();
        }

        public int UpdateKeyTime(int index, float time)
        {
            if (time < 0)
            {
                time = 0;
            }
            else if (time > 1)
            {
                time = 1;
            }

            if (index < 0) index = 0;

            Color col = Keys[index].Color;
            RemoveKey(index, false);
            return AddKey(col, time, false);
        }

        public void UpdateKeyColor(int index, Color col)
        {
            Keys[index] = new ColorKey(col, Keys[index].Time);
            UpdateTexture();
        }

        public Texture2D GetTexture()
        {
            return _texture;
        }

        public void UpdateTextureWidth(int width)
        {
            _texture = new Texture2D(width, 1, TextureFormat.RGB24, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
            UpdateTexture();
        }

        public void UpdateTexture()
        {
            Color[] colors = new Color[_texture.width];
            for (int i = 0; i < _texture.width; i++)
            {
                colors[i] = Evaluate((float)i / (_texture.width - 1));
            }
            _texture.SetPixels(colors);
            _texture.Apply(true);
        }

        [Serializable]
        public struct ColorKey
        {
            [SerializeField]
            public Color Color;
            
            [SerializeField]
            public float Time;

            public ColorKey(Color color, float time)
            {
                Color = color;
                Time = time;
            }
        }
    }
}
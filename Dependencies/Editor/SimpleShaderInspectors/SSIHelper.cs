using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public static class SSIHelper
    {
        public static void FetchProperties(this IEnumerable<SimpleControl> controls, MaterialProperty[] properties)
        {
            foreach (var control in controls)
            {
                if (control is PropertyControl pr)
                    pr.FetchProperty(properties);

                if (control is IAdditionalProperties add)
                    foreach (var t in add.AdditionalProperties)
                        t.FetchProperty(properties);

                if (control is IControlContainer con) 
                    con.GetControlList().FetchProperties(properties);
            }
        }

        public static void FetchProperties(this IEnumerable<SimpleControl> controls, MaterialProperty[] properties, out List<string> missingProperties)
        {
            var errs = new List<string>();
            foreach (var control in controls)
            {
                if (control is PropertyControl pr)
                {
                    pr.FetchProperty(properties);
                    if(pr.Property == null && !pr.PropertyName.Equals("SSI_UNUSED_PROP"))
                        errs.Add(pr.PropertyName);
                }

                if (control is IAdditionalProperties add)
                {
                    foreach (var t in add.AdditionalProperties)
                    {
                        t.FetchProperty(properties);
                        if(t.Property == null && t.IsPropertyMandatory)
                            errs.Add(t.PropertyName);
                    }
                }

                if (control is IControlContainer con)
                {
                    con.GetControlList().FetchProperties(properties, out List<string> ms);
                    errs.AddRange(ms);
                }
            }
            missingProperties = errs;
        }

        public static void SetInspector(this IEnumerable<SimpleControl> controls, ISimpleShaderInspector inspector, bool recursive = true)
        {
            foreach (var control in controls)
            {
                control.Inspector = inspector;
                if (recursive && control is IControlContainer con)
                    con.GetControlList().SetInspector(inspector);
            }
        }

        internal static int FindPropertyIndex(string propertyName, MaterialProperty[] properties, bool propertyIsMandatory = false)
        {
            if (!string.IsNullOrWhiteSpace(propertyName) && propertyName.Equals("SSI_UNUSED_PROP")) return -1;
            
            for (int i = 0; i < properties.Length; i++)
                if (properties[i] != null && properties[i].name == propertyName)
                    return i;

            if (propertyIsMandatory)
                throw new ArgumentException("Could not find MaterialProperty: '" + propertyName + "', Num properties: " + properties.Length);

            return -1;
        }

        public static IEnumerable<INonAnimatableProperty> FindNonAnimatablePropertyControls(this IEnumerable<SimpleControl> controls)
        {
            List<INonAnimatableProperty> nonAnimatablePropertyControls = new List<INonAnimatableProperty>();
            foreach(var control in controls)
            {
                if(control is INonAnimatableProperty c)
                    nonAnimatablePropertyControls.Add(c);

                if(control is IControlContainer container)
                    nonAnimatablePropertyControls.AddRange(container.GetControlList().FindNonAnimatablePropertyControls());
            }
            return nonAnimatablePropertyControls;
        }

        public static void UpdateNonAnimatableProperties(IEnumerable<INonAnimatableProperty> controls, MaterialEditor materialEditor, bool updateOutsideAnimation = true)
        {
            List<INonAnimatableProperty> propertiesNeedingUpdate = new List<INonAnimatableProperty>();
            foreach(var control in controls)
                if (control.NonAnimatablePropertyChanged)
                    propertiesNeedingUpdate.Add(control);

            if (propertiesNeedingUpdate.Count == 0) return;

            if (updateOutsideAnimation)
            {
                var editorAssembly = typeof(Editor).Assembly;
                var windowType = editorAssembly.GetType("UnityEditorInternal.AnimationWindowState");

                var isRecordingProp = windowType.GetProperty
                    ("recording", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

                UnityEngine.Object[] windowInstances = Resources.FindObjectsOfTypeAll(windowType);
                UnityEngine.Object recordingInstance = null;

                foreach (var t in windowInstances)
                {
                    bool isRecording = isRecordingProp != null && (bool)isRecordingProp.GetValue
                        (t, null);

                    if (!isRecording) continue;
                    recordingInstance = t;
                    break;
                }
                if (recordingInstance != null)
                {
                    System.Reflection.MethodBase stopRecording = windowType.GetMethod
                        ("StopRecording", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    System.Reflection.MethodBase startRecording = windowType.GetMethod
                        ("StartRecording", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

                    stopRecording?.Invoke(recordingInstance, null);
                    SetNonAnimatableProperties(materialEditor, propertiesNeedingUpdate);
                    startRecording?.Invoke(recordingInstance, null);
                }
                else
                {
                    SetNonAnimatableProperties(materialEditor, propertiesNeedingUpdate);
                }
            }
            else
            {
                SetNonAnimatableProperties(materialEditor, propertiesNeedingUpdate);
            }
        }
        
        private static void SetNonAnimatableProperties(MaterialEditor materialEditor, IEnumerable<INonAnimatableProperty> nonAnimatableProperties)
        {
            foreach(var nonAnimatableProperty in nonAnimatableProperties)
            {
                nonAnimatableProperty.UpdateNonAnimatableProperty(materialEditor);
                nonAnimatableProperty.NonAnimatablePropertyChanged = false;
            }
        }

        public static string GetTextureDestinationPath(Material mat, string name)
        {
            string path = AssetDatabase.GetAssetPath(mat);
            path = Directory.GetParent(path)?.FullName;
            string pathParent = Directory.GetParent(path)?.FullName;

            if (Directory.Exists(pathParent + "/Textures/"))
                return pathParent + "/Textures/" + mat.name + name;
            else
                return path + "/" + mat.name + name;
        }

        public static void SaveTexture(Texture2D texture, string path, TextureWrapMode mode = TextureWrapMode.Repeat, bool linear = false)
        {
            byte[] bytes = texture.EncodeToPNG();

            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
            path = path.Substring(path.LastIndexOf("Assets", StringComparison.Ordinal));
            var t = AssetImporter.GetAtPath(path) as TextureImporter;
            if (t != null)
            {
                t.wrapMode = mode;
                t.isReadable = true;
                t.sRGBTexture = !linear;
            }

            AssetDatabase.ImportAsset(path);
        }

        public static Texture2D SaveAndGetTexture(Texture2D texture, string path, TextureWrapMode mode = TextureWrapMode.Repeat, bool linear = false)
        {
            SaveTexture(texture, path, mode, linear);
            path = path.Substring(path.LastIndexOf("Assets", StringComparison.Ordinal));
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public static void SetTextureImporterReadable(Texture2D texture, bool isReadable)
        {
            if (texture is null) return;

            string assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter is null) return;
            
            tImporter.textureType = TextureImporterType.Default;
            tImporter.isReadable = isReadable;
            AssetDatabase.ImportAsset(assetPath);
        }
        
        public static bool IsSrgb(this Texture2D texture)
        {
            switch (texture.graphicsFormat)
            {
                case GraphicsFormat.R8_SRGB:
                case GraphicsFormat.R8G8_SRGB:
                case GraphicsFormat.R8G8B8_SRGB:
                case GraphicsFormat.R8G8B8A8_SRGB:
                case GraphicsFormat.B8G8R8_SRGB:
                case GraphicsFormat.B8G8R8A8_SRGB:
                case GraphicsFormat.RGBA_DXT3_SRGB:
                case GraphicsFormat.RGBA_DXT5_SRGB:
                case GraphicsFormat.RGBA_BC7_SRGB:
                case GraphicsFormat.RGB_PVRTC_2Bpp_SRGB:
                case GraphicsFormat.RGB_PVRTC_4Bpp_SRGB:
                case GraphicsFormat.RGBA_PVRTC_2Bpp_SRGB:
                case GraphicsFormat.RGBA_PVRTC_4Bpp_SRGB:
                case GraphicsFormat.RGB_ETC2_SRGB:
                case GraphicsFormat.RGB_A1_ETC2_SRGB:
                case GraphicsFormat.RGBA_ETC2_SRGB:
                case GraphicsFormat.RGBA_ASTC4X4_SRGB:
                case GraphicsFormat.RGBA_ASTC5X5_SRGB:
                case GraphicsFormat.RGBA_ASTC6X6_SRGB:
                case GraphicsFormat.RGBA_ASTC8X8_SRGB:
                case GraphicsFormat.RGBA_ASTC10X10_SRGB:
                case GraphicsFormat.RGBA_ASTC12X12_SRGB:
                    return true;
                default:
                    return false;
            }
        }
        
        public static void SetTextureImporterAlpha(Texture2D texture, bool alphaIsTransparency)
        {
            if (texture is null) return;

            string assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter is  null) return;
            
            tImporter.textureType = TextureImporterType.Default;
            tImporter.alphaIsTransparency = alphaIsTransparency;
            AssetDatabase.ImportAsset(assetPath);
        }
    }
}
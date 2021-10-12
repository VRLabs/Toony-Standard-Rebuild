using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    /// <summary>
    /// Static class containing dictionaries that can be used by controls for various needs.
    /// </summary>
    public static class StaticDictionaries
    {
        private static SerializedDictionaries _dictionaries;
        private static TimedDictionary<string, bool> _boolDictionary;
        /// <summary>
        /// Dictionary containing boolean values.
        /// </summary>
        public static TimedDictionary<string, bool> BoolDictionary
        {
            get
            {
                if (_boolDictionary != null) return _boolDictionary;
                LoadDictionaries();
                return _boolDictionary;

            }
        }

        private static TimedDictionary<string, int> _intDictionary;
        /// <summary>
        /// Dictionary containing integer values.
        /// </summary>
        public static TimedDictionary<string, int> IntDictionary
        {
            get
            {
                if (_intDictionary != null) return _intDictionary;
                LoadDictionaries();
                return _intDictionary;
            }
        }

        private static void LoadDictionaries()
        {
            _dictionaries = Resources.Load<SerializedDictionaries>("Dictionaries/TSRSSIDictionaries");

            if (_dictionaries == null)
            {
                _dictionaries = ScriptableObject.CreateInstance<SerializedDictionaries>();
                Directory.CreateDirectory("Assets/Resources/Dictionaries");
                AssetDatabase.CreateAsset(_dictionaries, "Assets/Resources/Dictionaries/TSRSSIDictionaries.asset");
                AssetDatabase.SaveAssets();
            }

            _boolDictionary = new TimedDictionary<string, bool>();
            foreach (SerializedDictionaries.BoolItem item in _dictionaries.boolDictionary)
                _boolDictionary.SetValue(item.key, item.value, DateTime.FromBinary(item.date));

            _intDictionary = new TimedDictionary<string, int>();
            foreach (SerializedDictionaries.IntItem item in _dictionaries.intDictionary)
                _intDictionary.SetValue(item.key, item.value, DateTime.FromBinary(item.date));
        }

        [InitializeOnLoad]
        public class Startup
        {
            static Startup() => EditorApplication.quitting += SaveAsset;

            private static void SaveAsset()
            {
                if (_dictionaries != null)
                {
                    StaticDictionaries.BoolDictionary.ClearOld();
                    StaticDictionaries.IntDictionary.ClearOld();
                    _dictionaries.SetBoolDictionary(StaticDictionaries.BoolDictionary.GetSerializedDictionary());
                    _dictionaries.SetIntDictionary(StaticDictionaries.IntDictionary.GetSerializedDictionary());
                    EditorUtility.SetDirty(_dictionaries);
                }
            }
        }
    }
}
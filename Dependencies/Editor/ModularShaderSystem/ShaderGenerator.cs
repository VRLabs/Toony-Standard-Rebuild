using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.ModularShaderSystem
{
    public static class ShaderGenerator
    {
        public static void GenerateShader(string path, ModularShader shader, bool hideVariants = false)
        {
            GenerateShader(path, shader, null,hideVariants);
        }

        public static void GenerateShader(string path, ModularShader shader, Action<StringBuilder, ShaderContext> postGeneration, bool hideVariants = true)
        {
            var modules = FindAllModules(shader);
            
            var freshAssets = new Dictionary<TemplateAsset, TemplateAsset>();

            freshAssets.AddFreshShaderToList(shader.ShaderTemplate);
            freshAssets.AddFreshShaderToList(shader.ShaderPropertiesTemplate);

            foreach (var template in modules.SelectMany(x => x.Templates))
                freshAssets.AddFreshShaderToList(template.Template);

            foreach (var function in modules.SelectMany(x => x.Functions))
                freshAssets.AddFreshShaderToList(function.ShaderFunctionCode);
            
            
            var possibleVariants = GetShaderVariants(modules);
            var contexts = new List<ShaderContext>();
            var completePropertiesBlock = GetPropertiesBlock(shader, modules, freshAssets);
            
            foreach (var variant in possibleVariants)
            {
                contexts.Add(new ShaderContext
                {
                    Shader = shader,
                    PostGeneration = postGeneration,
                    ActiveEnablers = variant,
                    FreshAssets = freshAssets,
                    FilePath = path,
                    PropertiesBlock = completePropertiesBlock,
                    AreVariantsHidden = true
                });
            }
            
            contexts.AsParallel().ForAll(x => x.GenerateShader());
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var context in contexts)
                    File.WriteAllText($"{path}/" + context.VariantFileName, context.ShaderFile.ToString());
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            
            AssetDatabase.Refresh();
            shader.LastGeneratedShaders = new List<Shader>();
            foreach (var context in contexts)
                shader.LastGeneratedShaders.Add(AssetDatabase.LoadAssetAtPath<Shader>($"{path}/" + context.VariantFileName));
        }

        public static void GenerateMinimalShader(string path, ModularShader shader, IEnumerable<Material> materials)
        {
            GenerateMinimalShader(path, shader, materials, null);
        }

        public static void GenerateMinimalShader(string path, ModularShader shader, IEnumerable<Material> materials, Action<StringBuilder, ShaderContext> postGeneration)
        {
            var modules = FindAllModules(shader);
            var possibleVariants = GetMinimalVariants(modules, materials);
            var contexts = new List<ShaderContext>();
            
            foreach (var (variant, material) in possibleVariants)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(material, out string guid, out long  _);
                contexts.Add(new ShaderContext
                {
                    Shader = shader,
                    PostGeneration = postGeneration,
                    ActiveEnablers = variant,
                    FilePath = path,
                    OptimizedShader = true,
                    Material = material,
                    Guid = guid
                });
            }

            contexts.GenerateMinimalShaders();
        }

        public static List<ShaderContext> EnqueueShadersToGenerate(string path, ModularShader shader, IEnumerable<Material> materials)
        {
            return EnqueueShadersToGenerate(path, shader, materials, null);
        }

        public static List<ShaderContext> EnqueueShadersToGenerate(string path, ModularShader shader, IEnumerable<Material> materials, Action<StringBuilder, ShaderContext> postGeneration)
        {
            var modules = FindAllModules(shader);
            var possibleVariants = GetMinimalVariants(modules, materials);
            var contexts = new List<ShaderContext>();
            
            foreach (var (variant, material) in possibleVariants)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(material, out string guid, out long  _);
                contexts.Add(new ShaderContext
                {
                    Shader = shader,
                    PostGeneration = postGeneration,
                    ActiveEnablers = variant,
                    FilePath = path,
                    OptimizedShader = true,
                    Material = material,
                    Guid = guid
                });
            }

            return contexts;
        }

        public static void GenerateMinimalShaders(this List<ShaderContext> contexts)
        {
            if (contexts == null || contexts.Count == 0) return;

            var alreadyDoneShaders = new List<ModularShader>();
            
            var freshAssets = new Dictionary<TemplateAsset, TemplateAsset>();
            
            foreach (var context in contexts)
            {
                context.FreshAssets = freshAssets;
                if (alreadyDoneShaders.Contains(context.Shader)) continue;
                
                var shader = context.Shader;
                var modules = FindAllModules(shader);
                
                freshAssets.AddFreshShaderToList(shader.ShaderTemplate);
                freshAssets.AddFreshShaderToList(shader.ShaderPropertiesTemplate);

                foreach (var template in modules.SelectMany(x => x.Templates))
                    freshAssets.AddFreshShaderToList(template.Template);

                foreach (var function in modules.SelectMany(x => x.Functions))
                    freshAssets.AddFreshShaderToList(function.ShaderFunctionCode);
                
                alreadyDoneShaders.Add(shader);
            }
            
            EditorUtility.DisplayProgressBar("Generating Optimized Shaders", "generating shader files", 1 / (contexts.Count + 3));
            contexts.AsParallel().ForAll(x => x.GenerateShader());
            try
            {
                AssetDatabase.StartAssetEditing();
                int i = 0;
                foreach (var context in contexts)
                {
                    EditorUtility.DisplayProgressBar("Generating Optimized Shaders", "Saving " + context.VariantFileName, 1 + i / (contexts.Count + 3));
                    File.WriteAllText($"{context.FilePath}/" + context.VariantFileName, context.ShaderFile.ToString());
                    i++;
                }
            }
            finally
            {
                EditorUtility.DisplayProgressBar("Generating Optimized Shaders", "waiting for unity to compile shaders", contexts.Count - 2 / (contexts.Count + 3));
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            EditorUtility.DisplayProgressBar("Generating Optimized Shaders", "applying shaders to materials", contexts.Count - 1 / (contexts.Count + 3));
            foreach (var context in contexts)
            {
                context.Material.shader = Shader.Find(context.ShaderName);
            }
            
            EditorUtility.ClearProgressBar();
        }

        private static List<Dictionary<string, int>> GetShaderVariants(List<ShaderModule> modules)
        {
            var dictionary = new Dictionary<string, List<int>>();
            foreach (ShaderModule module in modules)
            {
                if (module == null || module.Enabled == null || 
                    string.IsNullOrWhiteSpace(module.Enabled.Name) || 
                    !(module.Templates?.Any(x => x.NeedsVariant) ?? false)) continue;

                if (dictionary.ContainsKey(module.Enabled.Name))
                    dictionary[module.Enabled.Name].Add(module.Enabled.EnableValue);
                else
                    dictionary.Add(module.Enabled.Name, new List<int>(new []{module.Enabled.EnableValue}));
            }

            var keys = dictionary.Keys.ToList();

            foreach (KeyValuePair<string,List<int>> keyValuePair in dictionary)
                if(!keyValuePair.Value.Contains(0))
                    keyValuePair.Value.Insert(0,0);

            var states = new List<Dictionary<string, int>>();
            UnrollVariants(states, new Dictionary<string, int>(), dictionary, keys);

            return states;
        }
        
        private static List<(Dictionary<string, int>, Material)> GetMinimalVariants(List<ShaderModule> modules, IEnumerable<Material> materials)
        {
            var enablers = new List<string>();
            var dictionary = new Dictionary<string, int>();
            foreach (ShaderModule module in modules)
            {
                if (module == null || module.Enabled == null || 
                    string.IsNullOrWhiteSpace(module.Enabled.Name)) continue;

                enablers.Add(module.Enabled.Name);
            }

            enablers = enablers.Distinct().ToList();

            var states = new List<(Dictionary<string, int>, Material)>();
            foreach (Material material in materials)
            {
                var state = new Dictionary<string, int>();
                foreach (string enabler in enablers)
                    state.Add(enabler, (int)material.GetFloat(enabler));
                
                states.Add((state, material));
            }

            return states;
        }

        private static void UnrollVariants(ICollection<Dictionary<string, int>> states, Dictionary<string, int> current, IReadOnlyDictionary<string, List<int>> dictionary, IReadOnlyList<string> keys)
        {
            if (current.Count == keys.Count)
            {
                states.Add(current);
                return;
            }
            foreach (var value in dictionary[keys[current.Count]])
            {
                var next = new Dictionary<string, int>(current);
                next[keys[current.Count]] = value;
                UnrollVariants(states, next, dictionary, keys);
            }
        }

        public static string GetVariantCode(Dictionary<string, int> activeEnablers)
        {
            var keys = activeEnablers.Keys.OrderBy(x => x).ToList();
            bool isAllZeroes = true;
            var b = new StringBuilder();
            foreach (string key in keys)
            {
                if (activeEnablers[key] != 0) isAllZeroes = false;
                b.Append($"-{activeEnablers[key]}");
            }

            return isAllZeroes ? "" : b.ToString();
        }
        
        private static void AddFreshShaderToList(this Dictionary<TemplateAsset, TemplateAsset> dictionary, TemplateAsset asset)
        {
            if ((object)asset == null) return;
            if (dictionary.ContainsKey(asset)) return;
            string assetPath = AssetDatabase.GetAssetPath(asset);
            var genericAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            TemplateAsset template = null;
            switch (genericAsset)
            {
                case TemplateCollectionAsset collection:
                    template = collection.Templates.FirstOrDefault(x => x.name.Equals(asset.name));
                    break;
                case TemplateAsset t:
                    template = t;
                    break;
            }
            dictionary.Add(asset, template);
        }
        
        private static TemplateAsset GetTemplate(this Dictionary<TemplateAsset, TemplateAsset> dictionary, TemplateAsset asset)
        {
            if ((object)asset == null) return null;
            return dictionary.TryGetValue(asset, out TemplateAsset result) ? result : null;
        }

        public class ShaderContext
        {
            public ModularShader Shader;
            public Dictionary<string, int> ActiveEnablers;
            public Dictionary<TemplateAsset, TemplateAsset> FreshAssets;
            public Action<StringBuilder, ShaderContext> PostGeneration;
            private List<EnableProperty> _liveUpdateEnablers;
            public string FilePath;
            public string VariantFileName;
            public string VariantName;
            public string ShaderName;
            public string PropertiesBlock;
            public bool AreVariantsHidden;
            public bool OptimizedShader;
            public Material Material;
            public StringBuilder ShaderFile;
            private List<ShaderModule> _modules;
            private List<ShaderFunction> _functions;
            private List<ShaderFunction> _reorderedFunctions;
            private Dictionary<ShaderFunction, ShaderModule> _modulesByFunctions;
            public string Guid;

            public void GenerateShader()
            {
                _modules = FindActiveModules(Shader, ActiveEnablers);
                GetLiveUpdateEnablers();
                ShaderFile = new StringBuilder();
                VariantName = GetVariantCode(ActiveEnablers);
                if (OptimizedShader)
                {
                    VariantFileName = $"{Shader.Name}{(string.IsNullOrEmpty(VariantName) ? "" : $"-g-{Guid}")}.shader";
                }
                else
                {
                    VariantFileName = $"{Shader.Name}{(string.IsNullOrEmpty(VariantName) ? "" : $"-v{VariantName}")}.shader";
                }

                VariantFileName = string.Join("_", VariantFileName.Split(Path.GetInvalidFileNameChars()));

                if (OptimizedShader)
                    ShaderName = $"Hidden/{Shader.ShaderPath}-g-{Guid}";
                else if (AreVariantsHidden && !string.IsNullOrEmpty(VariantName))
                    ShaderName = $"Hidden/{Shader.ShaderPath}-v{VariantName}";
                else
                    ShaderName = $"{Shader.ShaderPath}{VariantName}";
                
                ShaderFile.AppendLine($"Shader \"{ShaderName}\"");

                ShaderFile.AppendLine("{");

                if (string.IsNullOrEmpty(PropertiesBlock))
                    ShaderFile.Append(GetPropertiesBlock(Shader, _modules, FreshAssets, false));
                else
                    ShaderFile.Append(PropertiesBlock);
                    
                WriteShaderSkeleton();
                
                _functions = new List<ShaderFunction>();
                _reorderedFunctions = new List<ShaderFunction>();
                _modulesByFunctions = new Dictionary<ShaderFunction, ShaderModule>();
                foreach (var module in _modules)
                {
                    _functions.AddRange(module.Functions);
                    foreach (ShaderFunction function in module.Functions)
                    {
                        _modulesByFunctions.Add(function, module);
                    }
                }

                WriteVariablesToKeywords();
                WriteFunctionCallsToKeywords();
                WriteFunctionsToKeywords();

                if (!string.IsNullOrWhiteSpace(Shader.CustomEditor))
                    ShaderFile.AppendLine($"CustomEditor \"{Shader.CustomEditor}\"");
                ShaderFile.AppendLine("}");

                PostGeneration?.Invoke(ShaderFile, this);

                MatchCollection m = Regex.Matches(ShaderFile.ToString(), @"#K#.*$", RegexOptions.Multiline);
                for (int i = m.Count - 1; i >= 0; i--)
                    ShaderFile.Replace(m[i].Value, "");

                ShaderFile.Replace("\r\n", "\n");

                if (OptimizedShader)
                {
                    ShaderFile = CleanupShaderFile(ShaderFile);
                }
                else
                {
                    ShaderFile = CleanupShaderFile(ShaderFile);
                }
            }
            
            private void GetLiveUpdateEnablers()
            {
                _liveUpdateEnablers = new List<EnableProperty>();
                var staticEnablers = ActiveEnablers.Keys.ToList();
                foreach (ShaderModule module in _modules)
                {
                    if(module.Enabled != null && !string.IsNullOrWhiteSpace(module.Enabled.Name) && !staticEnablers.Contains(module.Enabled.Name))
                        _liveUpdateEnablers.Add(module.Enabled);
                }

                _liveUpdateEnablers = _liveUpdateEnablers.Distinct().ToList();
            }

            private void WriteFunctionCallsToKeywords()
            {
                foreach (var startKeyword in _functions.Where(x => x.AppendAfter?.StartsWith("#K#") ?? false).Select(x => x.AppendAfter).Distinct())
                {
                    if (!ShaderFile.Contains(startKeyword)) continue;

                    var callSequence = new StringBuilder();
                    WriteFunctionCallSequence(callSequence, startKeyword);
                    callSequence.AppendLine(startKeyword);
                    ShaderFile.Replace(startKeyword, callSequence.ToString());
                }
            }

            private void WriteShaderSkeleton()
            {
                ShaderFile.AppendLine("SubShader");
                ShaderFile.AppendLine("{");

                ShaderFile.AppendLine(FreshAssets.GetTemplate(Shader.ShaderTemplate).Template);

                Dictionary<ModuleTemplate, ShaderModule> moduleByTemplate = new Dictionary<ModuleTemplate, ShaderModule>();
                Dictionary<(string, string), string> convertedKeyword = new Dictionary<(string, string), string>();
                int InstanceCounter = 0;

                foreach (var module in _modules)
                    foreach (var template in module.Templates)
                        moduleByTemplate.Add(template, module);

                    foreach (var template in _modules.SelectMany(x => x.Templates).OrderBy(x => x.Queue))
                    {
                        var freshTemplate = FreshAssets.GetTemplate(template.Template);
                        var module = moduleByTemplate[template];
                        if (freshTemplate == null) continue;
                        bool hasEnabler = module.Enabled != null && !string.IsNullOrEmpty(module.Enabled.Name);
                        bool isFilteredIn = hasEnabler && ActiveEnablers.TryGetValue(module.Enabled.Name, out _);
                        bool needsIf = hasEnabler && !isFilteredIn && !template.NeedsVariant;
                        var tmp = new StringBuilder();

                        if (!needsIf)
                        {
                            tmp.AppendLine(freshTemplate.Template);
                        }

                        else
                        {
                            tmp.AppendLine($"if({module.Enabled.Name} == {module.Enabled.EnableValue})");
                            tmp.AppendLine("{");
                            tmp.AppendLine(freshTemplate.Template);
                            tmp.AppendLine("}");
                        }
                        
                        MatchCollection mki = Regex.Matches(tmp.ToString(), @"#KI#\S*", RegexOptions.Multiline);
                        for (int i = mki.Count - 1; i >= 0; i--)
                        {
                            string newKeyword;
                            if (convertedKeyword.TryGetValue((module.Id, mki[i].Value), out string replacedKeyword))
                            {
                                newKeyword = replacedKeyword;
                            }
                            else
                            {
                                newKeyword = $"{mki[i].Value}{InstanceCounter++}";
                                convertedKeyword.Add((module.Id, mki[i].Value), newKeyword);
                            }
                            tmp.Replace(mki[i].Value, newKeyword);
                        }

                        foreach (var keyword in template.Keywords.Count == 0 ? new string[] { MSSConstants.DEFAULT_CODE_KEYWORD } : template.Keywords.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray())
                        {
                            MatchCollection m = Regex.Matches(ShaderFile.ToString(), $@"#K#{keyword}(\s|$)", RegexOptions.Multiline);
                            for (int i = m.Count - 1; i >= 0; i--)
                                ShaderFile.Insert(m[i].Index, tmp.ToString());

                            if (convertedKeyword.TryGetValue((module.Id, $@"#KI#{keyword}"), out string replacedKeyword))
                            {
                                m = Regex.Matches(ShaderFile.ToString(), $@"{replacedKeyword}(\s|$)", RegexOptions.Multiline);
                                for (int i = m.Count - 1; i >= 0; i--)
                                    ShaderFile.Insert(m[i].Index, tmp.ToString());   
                            }
                        }
                    }
                    MatchCollection mkr = Regex.Matches(ShaderFile.ToString(), @"#KI#\S*", RegexOptions.Multiline);
                    for (int i = mkr.Count - 1; i >= 0; i--)
                        ShaderFile.Replace(mkr[i].Value, "");
                
                ShaderFile.AppendLine("}");
            }
            
            private void WriteVariablesToKeywords()
            {
                var variableDeclarations = new Dictionary<string,List<Variable>>();

                foreach (ShaderFunction function in _functions)
                {
                    if (function.VariableKeywords.Count > 0)
                    {
                        foreach (string keyword in function.VariableKeywords)
                        {
                            if (!variableDeclarations.ContainsKey(keyword))
                                variableDeclarations.Add(keyword, new List<Variable>());

                            foreach (Variable variable in function.UsedVariables)
                                variableDeclarations[keyword].Add(variable);
                        }
                    }
                    else
                    {
                        if (!variableDeclarations.ContainsKey(MSSConstants.DEFAULT_VARIABLES_KEYWORD))
                            variableDeclarations.Add(MSSConstants.DEFAULT_VARIABLES_KEYWORD, new List<Variable>());

                        foreach (Variable variable in function.UsedVariables)
                            variableDeclarations[MSSConstants.DEFAULT_VARIABLES_KEYWORD].Add(variable);
                    }
                }

                foreach (var declaration in variableDeclarations)
                {
                    declaration.Value.AddRange(_liveUpdateEnablers.Select(x => x.ToVariable()));
                    var decCode = string.Join("\n", declaration.Value.Distinct().OrderBy(x => x.Type).Select(x => x.GetDefinition())) + "\n\n";
                    MatchCollection m = Regex.Matches(ShaderFile.ToString(), $@"#K#{declaration.Key}\s", RegexOptions.Multiline);
                    for (int i = m.Count - 1; i >= 0; i--)
                        ShaderFile.Insert(m[i].Index, decCode);   
                }
            }
            
            private void WriteFunctionsToKeywords()
            {
                var keywordedCode = new Dictionary<string,StringBuilder>();

                foreach (ShaderFunction function in _reorderedFunctions)
                {
                    var freshAsset = FreshAssets.GetTemplate(function.ShaderFunctionCode);
                    if (function.CodeKeywords.Count > 0)
                    {
                        foreach (string keyword in function.CodeKeywords)
                        {
                            if (!keywordedCode.ContainsKey(keyword))
                                keywordedCode.Add(keyword, new StringBuilder());

                            if (freshAsset != null) keywordedCode[keyword].AppendLine(freshAsset.Template);
                        }
                    }
                    else
                    {
                        if (!keywordedCode.ContainsKey(MSSConstants.DEFAULT_CODE_KEYWORD))
                            keywordedCode.Add(MSSConstants.DEFAULT_CODE_KEYWORD, new StringBuilder());
                        
                        if (freshAsset != null) keywordedCode[MSSConstants.DEFAULT_CODE_KEYWORD].AppendLine(freshAsset.Template);
                    }
                }

                foreach (var code in keywordedCode)
                {
                    MatchCollection m = Regex.Matches(ShaderFile.ToString(), $@"#K#{code.Key}\s", RegexOptions.Multiline);
                    for (int i = m.Count - 1; i >= 0; i--)
                        ShaderFile.Insert(m[i].Index, code.Value.ToString());   
                }
            }

            private void WriteFunctionCallSequence(StringBuilder callSequence, string appendAfter)
            {
                foreach (var function in _functions.Where(x => x.AppendAfter.Equals(appendAfter)).OrderBy(x => x.Queue))
                {
                    _reorderedFunctions.Add(function);
                    ShaderModule module = _modulesByFunctions[function];
                    
                    bool hasEnabler = module.Enabled != null && !string.IsNullOrEmpty(module.Enabled.Name);
                    bool isFilteredIn = hasEnabler && ActiveEnablers.TryGetValue(module.Enabled.Name, out _);
                    bool needsIf = hasEnabler && !isFilteredIn;

                    if (needsIf)
                    {
                        callSequence.AppendLine($"if({module.Enabled.Name} == {module.Enabled.EnableValue})");
                        callSequence.AppendLine("{");
                    }
                    
                    callSequence.AppendLine($"{function.Name}();");
                    WriteFunctionCallSequence(callSequence, function.Name);
                    
                    if (needsIf)
                        callSequence.AppendLine("}");
                }
            }
            
            private static bool CheckPropertyBlockLine(StringBuilder builder, StringReader reader, string line, ref int tabs, ref bool deleteEmptyLine)
            {
                string ln = null;
                line = line.Trim();
                if (string.IsNullOrEmpty(line))
                {
                    if (deleteEmptyLine)
                        return false;
                    deleteEmptyLine = true;
                }
                else
                {
                    deleteEmptyLine = false;
                }

                if (line.StartsWith("}") && (ln = reader.ReadLine()) != null && ln.Trim().StartsWith("SubShader"))
                    tabs--;
                builder.AppendLineTabbed(tabs, line);

                if (!string.IsNullOrWhiteSpace(ln))
                    if (CheckPropertyBlockLine(builder, reader, ln, ref tabs, ref deleteEmptyLine))
                        return true;

                if (line.StartsWith("}") && ln != null && ln.Trim().StartsWith("SubShader"))
                    return true;
                return false;
            }
            
            private static StringBuilder CleanupShaderFile(StringBuilder shaderVariant)
            {
                var finalFile = new StringBuilder(); ;
                using (var sr = new StringReader(shaderVariant.ToString()))
                {
                    string line;
                    int tabs = 0;
                    bool deleteEmptyLine = false;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();

                        if (string.IsNullOrEmpty(line))
                        {
                            if (deleteEmptyLine)
                                continue;
                            deleteEmptyLine = true;
                        }
                        else
                        {
                            deleteEmptyLine = false;
                        }

                        if (line.StartsWith("Properties"))
                        {
                            finalFile.AppendLineTabbed(tabs, line);
                            string ln = sr.ReadLine()?.Trim();      // When the previous line is the one containing "Properties" we always know 
                            finalFile.AppendLineTabbed(tabs, ln);   // that the next line is "{" so we just write it down before increasing the tabs
                            tabs++;
                            while ((ln = sr.ReadLine()) != null)    // we should be escaping this loop way before actually meeting the condition, but you never know
                            {
                                if (CheckPropertyBlockLine(finalFile, sr, ln, ref tabs, ref deleteEmptyLine))
                                    break;
                            }
                            continue;
                        }

                        if (line.StartsWith("}"))
                            tabs--;
                        finalFile.AppendLineTabbed(tabs, line);
                        if (line.StartsWith("{") || line.EndsWith("{"))
                            tabs++;
                    }
                }

                return finalFile;
            }
        }

        private static string GetPropertiesBlock(ModularShader shader, List<ShaderModule> modules, Dictionary<TemplateAsset, TemplateAsset> freshAssets, bool includeEnablers = true)
        {
            var block = new StringBuilder();
            block.AppendLine("Properties");
            block.AppendLine("{");

            if (shader.UseTemplatesForProperties)
            {
                var freshTemplate = freshAssets.GetTemplate(shader.ShaderPropertiesTemplate);
                if (freshTemplate != null)
                    block.AppendLine(freshTemplate.Template);

                block.AppendLine($"#K#{MSSConstants.TEMPLATE_PROPERTIES_KEYWORD}");
            }
            else
            {
                List<Property> properties = new List<Property>();

                properties.AddRange(shader.Properties.Where(x => !string.IsNullOrWhiteSpace(x.Name) || x.Attributes.Count > 0));

                foreach (var module in modules.Where(x => x != null))
                {
                    properties.AddRange(module.Properties.Where(x => !string.IsNullOrWhiteSpace(x.Name) || x.Attributes.Count > 0));
                    if (!string.IsNullOrWhiteSpace(module.Enabled.Name) && includeEnablers)
                        properties.Add(module.Enabled);
                }

                foreach (var prop in properties.Distinct())
                {
                    if (string.IsNullOrWhiteSpace(prop.Type) && !string.IsNullOrWhiteSpace(prop.Name))
                    {
                        prop.Type = "Float";
                        prop.DefaultValue = "0.0";
                    }

                    string attributes = prop.Attributes.Count == 0 ? "" : $"[{string.Join("][", prop.Attributes)}]";
                    block.AppendLine(string.IsNullOrWhiteSpace(prop.Name) ? attributes : $"{attributes} {prop.Name}(\"{prop.DisplayName}\", {prop.Type}) = {prop.DefaultValue}");
                }
            }

            block.AppendLine("}");
            return block.ToString();
        }
        
        public static List<ShaderModule> FindAllModules(ModularShader shader)
        {
            List<ShaderModule> modules = new List<ShaderModule>();
            if (shader == null) return modules;
            modules.AddRange(shader.BaseModules);
            modules.AddRange(shader.AdditionalModules);
            return modules;
        }

        public static List<Property> FindAllProperties(ModularShader shader)
        {
            List<Property> properties = new List<Property>();
            if (shader == null) return properties;

            properties.AddRange(shader.Properties.Where(x => !string.IsNullOrWhiteSpace(x.Name) || x.Attributes.Count == 0));

            foreach (var module in shader.BaseModules.Where(x => x != null))
            {
                properties.AddRange(module.Properties.Where(x => !string.IsNullOrWhiteSpace(x.Name) || x.Attributes.Count == 0));
                if (!string.IsNullOrWhiteSpace(module.Enabled.Name))
                    properties.Add(module.Enabled);
            }

            foreach (var module in shader.AdditionalModules.Where(x => x != null))
            {
                properties.AddRange(module.Properties.Where(x => !string.IsNullOrWhiteSpace(x.Name) || x.Attributes.Count == 0));
                if (!string.IsNullOrWhiteSpace(module.Enabled.Name))
                    properties.Add(module.Enabled);
            }

            return properties.Distinct().ToList();
        }
        
        public static List<ShaderFunction> FindAllFunctions(ModularShader shader)
        {
            var functions = new List<ShaderFunction>();
            if (shader == null) return functions;
            foreach (var module in shader.BaseModules)
                functions.AddRange(module.Functions);

            foreach (var module in shader.AdditionalModules)
                functions.AddRange(module.Functions);
            return functions;
        }
        
        public static List<ShaderModule> FindActiveModules(ModularShader shader, Dictionary<string, int> activeEnablers)
        {
            List<ShaderModule> modules = new List<ShaderModule>();
            if (shader == null) return modules;

            foreach (var module in shader.BaseModules)
            {
                int value = 0;
                bool hasEnabler = module.Enabled != null && !string.IsNullOrEmpty(module.Enabled.Name);
                bool hasKey = hasEnabler && activeEnablers.TryGetValue(module.Enabled.Name, out value);
                if (!hasEnabler || ((hasKey && module.Enabled.EnableValue == value) || !hasKey))
                    modules.Add(module);
            }

            foreach (var module in shader.AdditionalModules)
            {
                int value = 0;
                bool hasEnabler = module.Enabled != null && !string.IsNullOrEmpty(module.Enabled.Name);
                bool hasKey = hasEnabler && activeEnablers.TryGetValue(module.Enabled.Name, out value);
                if (!hasEnabler || ((hasKey && module.Enabled.EnableValue == value) || !hasKey))
                    modules.Add(module);
            }

            return modules;
        }

        public static List<string> CheckShaderIssues(ModularShader shader)
        {
            List<string> errors = new List<string>();
            var modules = FindAllModules(shader);

            for (int i = 0; i < modules.Count; i++)
            {
                var dependencies = new List<string>(modules[i].ModuleDependencies);
                for (int j = 0; j < modules.Count; j++)
                {
                    if (modules[j].IncompatibleWith.Any(x => x.Equals(modules[i].Id))) 
                        errors.Add($"Module \"{modules[j].Name}\" is incompatible with module \"{modules[i].name}\".");
                    
                    if (i != j && modules[i].Id.Equals(modules[j].Id))
                        errors.Add($"Module \"{modules[i].Name}\" is duplicate.");
                    
                    if (dependencies.Contains(modules[j].Id))
                        dependencies.Remove(modules[j].Id);
                }
                for (int j = 0; j < dependencies.Count; j++)
                    errors.Add($"Module \"{modules[i].Name}\" has missing dependency id \"{dependencies[j]}\".");
            }
            return errors;
        }
        
        public static List<string> CheckShaderIssues(List<ShaderModule> modules)
        {
            List<string> errors = new List<string>();

            for (int i = 0; i < modules.Count; i++)
            {
                var dependencies = new List<string>(modules[i].ModuleDependencies);
                for (int j = 0; j < modules.Count; j++)
                {
                    if (modules[j].IncompatibleWith.Any(x => x.Equals(modules[i].Id))) 
                        errors.Add($"Module \"{modules[j].Name}\" is incompatible with module \"{modules[i].name}\".");
                    
                    if (i != j && modules[i].Id.Equals(modules[j].Id))
                        errors.Add($"Module \"{modules[i].Name}\" is duplicate.");
                    
                    if (dependencies.Contains(modules[j].Id))
                        dependencies.Remove(modules[j].Id);
                }
                for (int j = 0; j < dependencies.Count; j++)
                    errors.Add($"Module \"{modules[i].Name}\" has missing dependency id \"{dependencies[j]}\".");
            }
            return errors;
        }
    }
}
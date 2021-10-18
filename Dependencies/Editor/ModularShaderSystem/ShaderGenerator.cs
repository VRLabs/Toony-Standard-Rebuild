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
    public enum VerificationResponse
    {
        NoIssues,
        DuplicateModule,
        MissingDependencies,
        IncompatibleModules
    }

    public static class ShaderGenerator
    {
        private static ModularShader _shader;
        private static List<ShaderModule> _modules;
        private static List<EnableProperty> _variantPropertyEnablers;
        private static List<string> _variantEnablerNames;
        private static List<Property> _properties;

        public static void GenerateMainShader(string path, ModularShader shader, bool hideVariants = false)
        {
            _shader = shader;
            var shaderFile = new StringBuilder();
            _modules = FindAllModules(_shader);
            _variantPropertyEnablers = _modules
                .Where(x => x != null && x.Templates?.Count(y => y.NeedsVariant) > 0 && (x.Enabled != null && !string.IsNullOrWhiteSpace(x.Enabled.Name)))
                .Select(x => x.Enabled).ToList();
            _variantEnablerNames = _variantPropertyEnablers.Select(x => x.Name).Distinct().OrderBy(x => x).ToList();
            _properties = FindAllProperties(_shader);

            WriteProperties(shaderFile);

            int currentlyIteratedObject = 0;

            EnablePropertyValue[] currentSettings = new EnablePropertyValue[_variantEnablerNames.Count];

            var variants = GenerateVariantsRecursive(shaderFile, currentlyIteratedObject, currentSettings, hideVariants);

            foreach ((string variantCode, StringBuilder shaderVariant) in variants)
            {
                StringBuilder finalFile = CleanupShaderFile(shaderVariant);
                File.WriteAllText($"{path}/" + string.Join("_", $"{_shader.Name}{variantCode}.shader".Split(Path.GetInvalidFileNameChars())), finalFile.ToString());
            }

            AssetDatabase.Refresh();

            _shader.LastGeneratedShaders = new List<Shader>();

            foreach ((string variantCode, StringBuilder _) in variants)
            {
                _shader.LastGeneratedShaders.Add(AssetDatabase.LoadAssetAtPath<Shader>($"{path}/" + string.Join("_", $"{_shader.Name}{variantCode}.shader".Split(Path.GetInvalidFileNameChars()))));
            }

            _shader = null;
            _modules = null;
            _variantPropertyEnablers = null;
            _variantEnablerNames = null;
            _properties = null;
        }

        public static void GenerateOptimizedShader(ModularShader shader, List<EnablePropertyValue> enableProperties)
        {
            _shader = shader;
            var shaderFile = new StringBuilder();
            _modules = FindAllModules(_shader);

            _properties = FindUsedProperties(_shader, enableProperties);

            List<ShaderModule> variantEnabledModules = _modules
                .Where(x => x.Enabled == null || string.IsNullOrWhiteSpace(x.Enabled.Name) || enableProperties.Select(y => y.Name).Contains(x.Enabled.Name))
                .ToList();

            string suffix = string.Join("", enableProperties.Select(x => x.Value));

            shaderFile.PrependLine("{");
            shaderFile.PrependLine($"Shader \"Hidden/opt/{_shader.ShaderPath}{suffix}\"");

            WriteProperties(shaderFile);

            WriteShaderSkeleton(shaderFile, enableProperties);

            var functions = new List<ShaderFunction>();
            foreach (var module in variantEnabledModules)
                functions.AddRange(module.Functions);

            WriteShaderVariables(shaderFile, variantEnabledModules, functions);

            WriteShaderFunctions(shaderFile, functions);

            shaderFile.AppendLine("}");

            File.WriteAllText($"{_shader.Name}{suffix}.shader", shaderFile.ToString());

            _modules = null;
            _properties = null;
        }

        public static VerificationResponse VerifyShaderModules(ModularShader shader)
        {
            var modules = FindAllModules(shader);
            var incompatibilities = modules.SelectMany(x => x.IncompatibleWith).Distinct().ToList();
            var dependencies = modules.SelectMany(x => x.ModuleDependencies).Distinct().ToList();

            for (int i = 0; i < modules.Count; i++)
            {
                if (incompatibilities.Any(x => x.Equals(modules[i].Id))) return VerificationResponse.IncompatibleModules;

                if (dependencies.Contains(modules[i].Id))
                    dependencies.Remove(modules[i].Id);

                for (int j = i + 1; j < modules.Count; j++)
                {
                    if (modules[i].Id.Equals(modules[j].Id))
                        return VerificationResponse.DuplicateModule;
                }
            }

            return dependencies.Count > 0 ? VerificationResponse.MissingDependencies : VerificationResponse.NoIssues;
        }

        public static List<ShaderModule> FindAllModules(ModularShader shader)
        {
            List<ShaderModule> modules = new List<ShaderModule>();
            if (shader == null) return modules;
            modules.AddRange(shader.BaseModules);
            modules.AddRange(shader.AdditionalModules);
            return modules;
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

        public static List<Property> FindUsedProperties(ModularShader shader, IEnumerable<EnablePropertyValue> values)
        {
            List<Property> properties = new List<Property>();

            properties.AddRange(shader.Properties);

            foreach (var module in shader.BaseModules.Where(x => x.Enabled == null || string.IsNullOrWhiteSpace(x.Enabled.Name) ||
                values.Count(y => y.Name.Equals(x.Enabled.Name) && y.Value == x.Enabled.EnableValue) > 0))
                properties.AddRange(module.Properties);

            foreach (var module in shader.AdditionalModules.Where(x => x.Enabled == null || string.IsNullOrWhiteSpace(x.Enabled.Name) ||
                values.Count(y => y.Name.Equals(x.Enabled.Name) && y.Value == x.Enabled.EnableValue) > 0))
                properties.AddRange(module.Properties);

            return properties.Distinct().ToList();
        }

        private static List<(string, StringBuilder)> GenerateVariantsRecursive(StringBuilder shaderFile, int currentlyIteratedObject, EnablePropertyValue[] currentSettings, bool isVariantHidden)
        {
            var files = new List<(string, StringBuilder)>();

            if (currentlyIteratedObject >= _variantEnablerNames.Count)
            {
                var variantShader = new StringBuilder(shaderFile.ToString());
                files.Add(GenerateShaderVariant(variantShader, currentSettings, isVariantHidden));
                return files;
            }

            List<int> possibleValues = _variantPropertyEnablers.Where(x => x.Name.Equals(_variantEnablerNames[currentlyIteratedObject]))
                .Select(x => x.EnableValue)
                .Append(0)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            foreach (int value in possibleValues)
            {
                var newSettings = new EnablePropertyValue[_variantEnablerNames.Count];
                Array.Copy(currentSettings, newSettings, _variantEnablerNames.Count);
                newSettings[currentlyIteratedObject] = new EnablePropertyValue { Name = _variantEnablerNames[currentlyIteratedObject], Value = value };
                List<(string, StringBuilder)> returnFiles = GenerateVariantsRecursive(shaderFile, currentlyIteratedObject + 1, newSettings, isVariantHidden);

                files.AddRange(returnFiles);
            }

            return files;
        }

        private static (string, StringBuilder) GenerateShaderVariant(StringBuilder shaderFile, EnablePropertyValue[] currentSettings, bool isVariantHidden)
        {
            string suffix = "";
            if (currentSettings.Any(x => x.Value != 0))
                suffix = string.Join("-", currentSettings.Select(x => x.Value));

            List<ShaderModule> variantEnabledModules = _modules
                .Where(x => x != null)
                .Where(x => x.Enabled == null || string.IsNullOrWhiteSpace(x.Enabled.Name) || !x.Templates.Any(y => y.NeedsVariant) || currentSettings.Any(y => x.Enabled.Name.Equals(y.Name) && x.Enabled.EnableValue == y.Value))
                .ToList();

            shaderFile.PrependLine("{");

            if (isVariantHidden)
                shaderFile.PrependLine($"Shader \"Hidden/{_shader.ShaderPath}{suffix}\"");
            else
                shaderFile.PrependLine($"Shader \"{_shader.ShaderPath}{suffix}\"");

            WriteShaderSkeleton(shaderFile, currentSettings);

            var functions = new List<ShaderFunction>();
            foreach (var module in variantEnabledModules)
                functions.AddRange(module.Functions);

            WriteShaderVariables(shaderFile, variantEnabledModules, functions);

            WriteShaderFunctions(shaderFile, functions);

            if (!string.IsNullOrWhiteSpace(_shader.CustomEditor))
                shaderFile.AppendLine($"CustomEditor \"{_shader.CustomEditor}\"");
            shaderFile.AppendLine("}");


            MatchCollection m = Regex.Matches(shaderFile.ToString(), @"#K#.*$", RegexOptions.Multiline);
            for (int i = m.Count - 1; i >= 0; i--)
                shaderFile.Replace(m[i].Value, "");

            shaderFile.Replace("\r\n", "\n");
            return (suffix, shaderFile);
        }

        private static void WriteShaderVariables(StringBuilder shaderFile, List<ShaderModule> variantEnabledModules, List<ShaderFunction> functions)
        {
            WriteVariablesToKeyword(shaderFile, variantEnabledModules, functions, MSSConstants.DEFAULT_VARIABLES_KEYWORD, true);
            foreach (var keyword in functions.SelectMany(x => x.VariableKeywords).Distinct().Where(x => !string.IsNullOrEmpty(x) && !x.Equals(MSSConstants.DEFAULT_VARIABLES_KEYWORD)))
                WriteVariablesToKeyword(shaderFile, variantEnabledModules, functions, keyword);
        }

        private static void WriteVariablesToKeyword(StringBuilder shaderFile, List<ShaderModule> variantEnabledModules, List<ShaderFunction> functions, string keyword, bool isDefaultKeyword = false)
        {
            var variablesDeclaration = new StringBuilder();
            foreach (var variable in functions
                .Where(x => x.VariableKeywords.Any(y => y.Equals(keyword)) || (isDefaultKeyword && x.VariableKeywords.Count == 0))
                .SelectMany(x => x.UsedVariables)
                .Concat(variantEnabledModules
                    .Where(x => x.Enabled != null && !string.IsNullOrWhiteSpace(x.Enabled.Name) && !x.Templates.Any(y => y.NeedsVariant))
                    .Select(x => x.Enabled.ToVariable()))
                .Distinct()
                .OrderBy(x => x.Type))
            {
                variablesDeclaration.AppendLine(variable.GetDefinition());
            }

            MatchCollection m = Regex.Matches(shaderFile.ToString(), $@"#K#{keyword}\s", RegexOptions.Multiline);
            for (int i = m.Count - 1; i >= 0; i--)
                shaderFile.Insert(m[i].Index, variablesDeclaration.ToString());
        }

        private static void WriteShaderFunctions(StringBuilder shaderFile, List<ShaderFunction> functions)
        {
            foreach (var function in functions.Where(x => x.AppendAfter.StartsWith("#K#")).OrderBy(x => x.Priority))
            {
                if (!shaderFile.Contains(function.AppendAfter)) continue;

                StringBuilder functionCode = new StringBuilder();
                StringBuilder functionCallSequence = new StringBuilder();
                int tabs = 2;
                tabs = WriteFunctionCallSequence(functions, function, functionCode, functionCallSequence, tabs);
                foreach (var codeKeyword in function.CodeKeywords.Count == 0 ? new string[] { MSSConstants.DEFAULT_CODE_KEYWORD } : function.CodeKeywords.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray())
                {
                    MatchCollection m = Regex.Matches(shaderFile.ToString(), $@"#K#{codeKeyword}\s", RegexOptions.Multiline);
                    for (int i = m.Count - 1; i >= 0; i--)
                        shaderFile.Insert(m[i].Index, functionCode.ToString());
                }

                functionCallSequence.AppendLine(function.AppendAfter);
                shaderFile.Replace(function.AppendAfter, functionCallSequence.ToString());
            }
        }

        private static int WriteFunctionCallSequence(List<ShaderFunction> functions, ShaderFunction function, StringBuilder functionCode, StringBuilder functionCallSequence, int tabs)
        {
            functionCode.AppendLine(function.ShaderFunctionCode.Template);

            ShaderModule module = _modules.Find(x => x.Functions.Contains(function));

            if (module.Enabled != null && !string.IsNullOrWhiteSpace(module.Enabled.Name) && !module.Templates.Any(x => x.NeedsVariant))
            {
                functionCallSequence.AppendLine($"if({module.Enabled.Name} == {module.Enabled.EnableValue})");
                functionCallSequence.AppendLine("{");
                tabs++;
                functionCallSequence.AppendLine($"{function.Name}();");
                foreach (var fn in functions.Where(x => x.AppendAfter.Equals(function.Name)).OrderBy(x => x.Priority))
                    WriteFunctionCallSequence(functions, fn, functionCode, functionCallSequence, tabs);

                functionCallSequence.AppendLine("}");
            }
            else
            {
                functionCallSequence.AppendLine($"{function.Name}();");
                foreach (var fn in functions.Where(x => x.AppendAfter.Equals(function.Name)).OrderBy(x => x.Priority))
                    WriteFunctionCallSequence(functions, fn, functionCode, functionCallSequence, tabs);
            }

            return tabs;
        }

        private static void WriteShaderSkeleton(StringBuilder shaderFile, IEnumerable<EnablePropertyValue> currentSettings)
        {
            shaderFile.AppendLine("SubShader");
            shaderFile.AppendLine("{");

            shaderFile.AppendLine(_shader.ShaderTemplate.Template);

            WriteModuleTemplates(shaderFile, currentSettings);
            shaderFile.AppendLine("}");
        }

        private static void WriteModuleTemplates(StringBuilder shaderFile, IEnumerable<EnablePropertyValue> currentSettings)
        {
            IEnumerable<EnablePropertyValue> enablePropertyValues = currentSettings as EnablePropertyValue[] ?? currentSettings.ToArray();
            foreach (var module in _modules.Where(x => x != null /*&& (string.IsNullOrWhiteSpace(x.Enabled.Name) || currentSettings.Select(y => y.Name).Contains(x.Enabled.Name))*/))
            {
                foreach (var template in module.Templates)
                {
                    if (template.Template == null) continue;
                    bool hasEnabler = !string.IsNullOrWhiteSpace(module.Enabled.Name);
                    bool isEnablerVariant = _variantEnablerNames.Contains(module.Enabled.Name);
                    var tmp = new StringBuilder();
                    if (!hasEnabler || (isEnablerVariant && enablePropertyValues.FirstOrDefault(x => x.Name.Equals(module.Enabled.Name)).Value == module.Enabled.EnableValue))
                    {
                        tmp.AppendLine(template.Template.ToString());
                    }
                    else if (!isEnablerVariant)
                    {
                        tmp.AppendLine($"if({module.Enabled.Name} == {module.Enabled.EnableValue})");
                        tmp.AppendLine("{");
                        tmp.AppendLine(template.Template.ToString());
                        tmp.AppendLine("}");
                    }

                    foreach (var keyword in template.Keywords.Count == 0 ? new string[] { MSSConstants.DEFAULT_CODE_KEYWORD } : template.Keywords.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray())
                    {
                        MatchCollection m = Regex.Matches(shaderFile.ToString(), $@"#K#{keyword}\s", RegexOptions.Multiline);
                        for (int i = m.Count - 1; i >= 0; i--)
                            shaderFile.Insert(m[i].Index, tmp.ToString());

                        m = Regex.Matches(shaderFile.ToString(), $@"#KI#{keyword}\s", RegexOptions.Multiline);
                        for (int i = m.Count - 1; i >= 0; i--)
                            shaderFile.Insert(m[i].Index, tmp.ToString());
                    }
                }
                MatchCollection mki = Regex.Matches(shaderFile.ToString(), @"#KI#.*$", RegexOptions.Multiline);
                for (int i = mki.Count - 1; i >= 0; i--)
                    shaderFile.Replace(mki[i].Value, "");
            }
        }

        private static void WriteProperties(StringBuilder shaderFile)
        {
            shaderFile.AppendLine("Properties");
            shaderFile.AppendLine("{");

            if (_shader.UseTemplatesForProperties)
            {
                if (_shader.ShaderPropertiesTemplate != null)
                    shaderFile.AppendLine(_shader.ShaderPropertiesTemplate.Template);

                shaderFile.AppendLine($"#K#{MSSConstants.TEMPLATE_PROPERTIES_KEYWORD}");
            }
            else
            {
                foreach (var prop in _properties)
                {
                    if (string.IsNullOrWhiteSpace(prop.Type) && !string.IsNullOrWhiteSpace(prop.Name))
                    {
                        prop.Type = "Float";
                        prop.DefaultValue = "0.0";
                    }

                    string attributes = prop.Attributes.Count == 0 ? "" : $"[{string.Join("][", prop.Attributes)}]";
                    shaderFile.AppendLine(string.IsNullOrWhiteSpace(prop.Name) ? attributes : $"{attributes} {prop.Name}(\"{prop.DisplayName}\", {prop.Type}) = {prop.DefaultValue}");
                }
            }

            shaderFile.AppendLine("}");
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
                        string ln = sr.ReadLine()?.Trim();               
                        finalFile.AppendLineTabbed(tabs, ln);   
                        tabs++;
                        while ((ln = sr.ReadLine()) != null)    
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
}
using System;
using System.IO;
using System.Text;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.ModularShaderSystem
{
    [ScriptedImporter(1, MSSConstants.TEMPLATE_COLLECTION_EXTENSION)]
    public class TemplateColletionAssetImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var subAsset = ScriptableObject.CreateInstance<TemplateCollectionAsset>();
            

            
            using (var sr = new StringReader(File.ReadAllText(ctx.assetPath)))
            {
                var builder = new StringBuilder();
                string line;
                string name = "";
                bool deleteEmptyLine = false;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("#T#"))
                    {
                        if (builder.Length > 0 && !string.IsNullOrWhiteSpace(name))
                            SaveSubAsset(ctx, subAsset, builder, name);
                        
                        builder = new StringBuilder();
                        name = line.Replace("#T#", "").Trim();
                        continue;
                    }

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

                    builder.AppendLine(line);
                }
                
                if (builder.Length > 0 && !string.IsNullOrWhiteSpace(name))
                    SaveSubAsset(ctx, subAsset, builder, name);
            }
            
            
            ctx.AddObjectToAsset("Collection", subAsset/*, icon*/); 
            ctx.SetMainObject(subAsset);
        }

        private static void SaveSubAsset(AssetImportContext ctx, TemplateCollectionAsset asset, StringBuilder builder, string name)
        {
            var templateAsset = ScriptableObject.CreateInstance<TemplateAsset>();
            templateAsset.Template = builder.ToString();
            templateAsset.name = name;
            ctx.AddObjectToAsset(name, templateAsset /*, icon*/); 
            asset.Templates.Add(templateAsset);
        }

        public override bool SupportsRemappedAssetType(Type type)
        {
            return type.IsAssignableFrom(typeof(TemplateAsset));
        }
    }
}
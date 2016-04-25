using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeferredPipeline
{
    [ContentTypeWriter]
    class CustomWriter : ContentTypeWriter<EffectMaterialContent>
    {
        protected override void Write(ContentWriter output, EffectMaterialContent value)
        {
            output.WriteExternalReference(value.CompiledEffect);
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (KeyValuePair<string, ExternalReference<TextureContent>> item in value.Textures)
            {
                dict.Add(item.Key, item.Value);
            }
            output.WriteObject<Dictionary<string, object>>(dict);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            var type = typeof(ContentReader);
            var readerType = type.Namespace + ".EffectMaterialReader, " + type.Assembly.FullName;
            // Console.WriteLine(readerType);
            return readerType;
        }
    }
}

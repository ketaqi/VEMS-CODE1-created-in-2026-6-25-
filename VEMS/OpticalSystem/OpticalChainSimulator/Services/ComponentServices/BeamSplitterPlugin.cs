using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System;
using System.Collections.Generic;
using System.Text.Json;
using OpticalChainSimulator.Models;

using OpticalChainSimulator.ViewModels;

namespace OpticalChainSimulator.Services.ComponentServices
{
    public class BeamSplitterPlugin : IOpticalElementPlugin
    {
        public string Id => "com.example.beamsplitter";
        public string Name => "Beam SplitterDAIMA";
        public string Version => "1.0.0";

        public JsonElement? Metadata => null;

        public IEnumerable<OpticalElement> GetTemplates()
        {
            var t = new OpticalElement
            {
                Id = Guid.NewGuid(),
                Type = OpticalElementType.Box, // 使用通用占位 Type
                Name = "BeamSplitter 50/50DAIMA",
                Parameters =
                {
                    ["Reflectance"] = 0.5,
                    ["Transmittance"] = 0.5,
                    ["Loss"] = 0.99
                },
                Properties = new Dictionary<string, JsonElement>()
            };

            // 标注插件来源
            using var doc = JsonDocument.Parse("\"com.example.beamsplitter\"");
            t.Properties["PluginId"] = doc.RootElement.Clone();
            using var doc2 = JsonDocument.Parse("\"BeamSplitter\"");
            t.Properties["PluginType"] = doc2.RootElement.Clone();

            yield return t;
        }

        public OpticalElementViewModel? CreateViewModel(OpticalElement model)
        {
            // 这里可以返回一个自定义的 OpticalElementViewModel 子类，
            // 也可以返回 null 以使用默认的 OpticalElementViewModel
            return null;
        }
    }
}
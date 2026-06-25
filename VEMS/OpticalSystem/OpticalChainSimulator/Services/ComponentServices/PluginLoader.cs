using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using OpticalChainSimulator.Models;

namespace OpticalChainSimulator.Services.ComponentServices
{
    /// <summary>
    /// 插件/清单加载器：
    /// - 扫描 plugins 目录的 DLL，加载实现 IOpticalElementPlugin 的类型；
    /// - 同时扫描 plugins 目录下的 JSON 清单文件（manifest），将其中声明的模板作为轻量“插件模板”加入库；
    /// 
    /// 设计要点：
    /// - manifest 支持无需编译 DLL 的模板扩展（使用 JSON 描述模板参数）；
    /// - manifest 模板为轻量模板（没有自定义 ViewModel），CreateCustomViewModel 对于 manifest 模板返回 null；
    /// - 为避免 yield 与 try/catch 的限制，GetAllTemplates 先收集后返回 List。
    /// </summary>
    public class PluginLoader
    {
        private readonly string _pluginFolder;
        public IList<IOpticalElementPlugin> LoadedPlugins { get; } = new List<IOpticalElementPlugin>();

        // manifest 来源产生的模板也会收集到这里（只是临时返回，不作为已注册插件项）
        private readonly List<OpticalElement> _manifestTemplates = new();

        public PluginLoader(string pluginFolder)
        {
            _pluginFolder = pluginFolder;
        }

        /// <summary>
        /// 扫描并加载插件（DLL + manifest JSON）。
        /// </summary>
        public void LoadPlugins()
        {
            // 1) 扫描 plugins 文件夹的 dll（外部插件）
            if (!Directory.Exists(_pluginFolder))
                Directory.CreateDirectory(_pluginFolder);

            var dlls = Directory.GetFiles(_pluginFolder, "*.dll", SearchOption.TopDirectoryOnly);
            foreach (var dll in dlls)
            {
                TryLoadAssembly(dll);
            }

            // 2) 还要扫描当前已加载的程序集（发现项目内部实现的插件）
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in assemblies)
                {
                    TryRegisterPluginsFromAssembly(asm);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Scan loaded assemblies failed: " + ex);
            }

            // 3) 扫描 JSON manifest（支持把模板以 JSON 形式放入 plugins 目录）
            try
            {
                LoadManifests();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Load manifests failed: " + ex);
            }

            System.Diagnostics.Debug.WriteLine($"PluginLoader: Loaded plugin count = {LoadedPlugins.Count}, manifest templates = {_manifestTemplates.Count}");
        }

        private void TryLoadAssembly(string path)
        {
            try
            {
                // 用独立的 AssemblyLoadContext 加载外部 dll（保持隔离）
                var alc = new AssemblyLoadContext(Path.GetFileNameWithoutExtension(path), isCollectible: true);
                using var fs = File.OpenRead(path);
                var asm = alc.LoadFromStream(fs);
                TryRegisterPluginsFromAssembly(asm);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load plugin assembly {path} failed: {ex}");
            }
        }

        private void TryRegisterPluginsFromAssembly(Assembly asm)
        {
            try
            {
                var pluginTypes = asm.GetTypes()
                    .Where(t => typeof(IOpticalElementPlugin).IsAssignableFrom(t) && !t.IsAbstract);

                foreach (var pt in pluginTypes)
                {
                    try
                    {
                        // 防止重复实例化同一插件 Type（按 FullName 判断）
                        if (LoadedPlugins.Any(p =>
                        {
                            try { return p.GetType().FullName == pt.FullName; } catch { return false; }
                        }))
                            continue;

                        if (Activator.CreateInstance(pt) is IOpticalElementPlugin plugin)
                        {
                            LoadedPlugins.Add(plugin);
                            System.Diagnostics.Debug.WriteLine($"PluginLoader: Registered plugin {plugin.Id} from {asm.FullName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Instantiate plugin type {pt.FullName} failed: {ex}");
                    }
                }
            }
            catch (ReflectionTypeLoadException rtlex)
            {
                System.Diagnostics.Debug.WriteLine($"Type load errors in assembly {asm.FullName}: {rtlex}");
                foreach (var le in rtlex.LoaderExceptions)
                    System.Diagnostics.Debug.WriteLine($" - loader exception: {le}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register plugins from assembly {asm.FullName} failed: {ex}");
            }
        }

        /// <summary>
        /// 读取 plugins 文件夹下的 JSON 文件，若 JSON 根对象包含 "id" 和 "templates" 字段，
        /// 则把其中 templates 解析为 OpticalElement 模板并加入 manifestTemplates。
        /// 支持任意文件名（不局限后缀），但建议以 *.plugin.json 或 *-plugin-manifest.json 命名以便识别。
        /// </summary>
        private void LoadManifests()
        {
            _manifestTemplates.Clear();

            var jsonFiles = Directory.GetFiles(_pluginFolder, "*.json", SearchOption.TopDirectoryOnly);
            foreach (var f in jsonFiles)
            {
                try
                {
                    using var sr = File.OpenRead(f);
                    using var doc = JsonDocument.Parse(sr);

                    var root = doc.RootElement;
                    if (!root.ValueKind.Equals(JsonValueKind.Object))
                        continue;

                    // 简单判定：必须包含 id 字段与 templates 数组
                    if (!root.TryGetProperty("id", out var idEl) || !root.TryGetProperty("templates", out var templatesEl))
                        continue;

                    var pluginId = idEl.GetString() ?? Path.GetFileNameWithoutExtension(f);

                    if (templatesEl.ValueKind != JsonValueKind.Array)
                        continue;

                    foreach (var tmplEl in templatesEl.EnumerateArray())
                    {
                        try
                        {
                            // name, type, parameters(optional)
                            var name = tmplEl.GetProperty("name").GetString() ?? "Unnamed";
                            var typeString = tmplEl.TryGetProperty("type", out var tEl) ? tEl.GetString() ?? "PluginElement" : "PluginElement";

                            var element = new OpticalElement
                            {
                                Id = Guid.NewGuid(),
                                Type = OpticalElementType.Box, // 使用通用占位类型以兼容现有代码
                                Name = name,
                                Parameters = new Dictionary<string, double>(),
                                Properties = new Dictionary<string, JsonElement>()
                            };

                            // parameters 可以是对象，含多个键和值（数值或其它）
                            if (tmplEl.TryGetProperty("parameters", out var paramsEl) && paramsEl.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var kv in paramsEl.EnumerateObject())
                                {
                                    // 如果是数字则写入 Parameters（double），否则写入 Properties（原始 JsonElement）
                                    var propName = kv.Name;
                                    var valEl = kv.Value;
                                    if (valEl.ValueKind == JsonValueKind.Number && valEl.TryGetDouble(out var d))
                                    {
                                        element.Parameters[propName] = d;
                                    }
                                    else if (valEl.ValueKind == JsonValueKind.String)
                                    {
                                        // 试图把可以解析为 double 的字符串转换
                                        var s = valEl.GetString();
                                        if (double.TryParse(s, out var d2))
                                            element.Parameters[propName] = d2;
                                        else
                                            element.Properties[propName] = valEl.Clone();
                                    }
                                    else
                                    {
                                        element.Properties[propName] = valEl.Clone();
                                    }
                                }
                            }

                            // 把 manifest 元信息写入 Properties（以便序列化时保留来源）
                            using (var pidDoc = JsonDocument.Parse($"\"{pluginId}\""))
                                element.Properties["PluginId"] = pidDoc.RootElement.Clone();
                            using (var ptypeDoc = JsonDocument.Parse($"\"{typeString}\""))
                                element.Properties["PluginType"] = ptypeDoc.RootElement.Clone();
                            using (var mfDoc = JsonDocument.Parse($"\"{Path.GetFileName(f)}\""))
                                element.Properties["PluginManifestFile"] = mfDoc.RootElement.Clone();

                            // 记录原始 manifest JSON（可选）
                            element.Properties["PluginManifestRaw"] = root.Clone();

                            _manifestTemplates.Add(element);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Parse template in manifest {f} failed: {ex}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Read manifest file {f} failed: {ex}");
                }
            }
        }

        /// <summary>
        /// 收集所有插件提供的模板（包括 DLL 插件提供的模板与 manifest 提供的轻量模板）。
        /// 返回一个列表供主程序注入到库（_libraryElements）。
        /// </summary>
        public IEnumerable<OpticalElement> GetAllTemplates()
        {
            var templates = new List<OpticalElement>();

            // 1) 来自 DLL / 程序集的插件模板
            foreach (var p in LoadedPlugins)
            {
                try
                {
                    var list = p.GetTemplates()?.ToList() ?? new List<OpticalElement>();
                    foreach (var t in list)
                    {
                        if (t.Properties == null)
                            t.Properties = new Dictionary<string, JsonElement>();

                        if (!t.Properties.ContainsKey("PluginId"))
                        {
                            using var doc = JsonDocument.Parse($"\"{p.Id}\"");
                            t.Properties["PluginId"] = doc.RootElement.Clone();
                        }

                        templates.Add(t);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Plugin {p.Id} GetTemplates() failed: {ex}");
                }
            }

            // 2) 来自 manifest 的轻量模板
            // 2) 来自 manifest 的轻量模板
            templates.AddRange(_manifestTemplates);

            return templates;
        }

        /// <summary>
        /// 尝试请求插件根据模型返回自定义 ViewModel（可选）。
        /// 注意：manifest 模板没有对应的 IOpticalElementPlugin 类型，因此无法提供自定义 VM（会返回 null）。
        /// </summary>
        public ViewModels.OpticalElementViewModel? CreateCustomViewModel(OpticalElement model)
        {
            if (model.Properties != null && model.Properties.TryGetValue("PluginId", out var pidEl))
            {
                try
                {
                    var pid = pidEl.GetString();
                    var plugin = LoadedPlugins.FirstOrDefault(x => x.Id == pid);
                    if (plugin != null)
                        return plugin.CreateViewModel(model);
                }
                catch { }
            }
            return null;
        }
    }
}
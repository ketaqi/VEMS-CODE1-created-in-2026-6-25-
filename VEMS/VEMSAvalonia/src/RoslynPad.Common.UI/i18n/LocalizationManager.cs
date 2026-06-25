using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Linq;
//using Avalonia.Data;
namespace RoslynPad;

//public class LocalizationManager : INotifyPropertyChanged
//{
//    public static LocalizationManager Instance { get; } = new LocalizationManager();
//    private Dictionary<string, string> _resources = new();

//    public event PropertyChangedEventHandler? PropertyChanged;

//    public string this[string key] => _resources.TryGetValue(key, out var value) ? value : key;

//    public void LoadLanguage(CultureInfo culture)
//    {
//        var filePath = $"i18n/MainModule.{culture.Name}.xml";
//        var xdoc = XDocument.Load(filePath);

//        _resources.Clear();
//        foreach (var element in xdoc.Descendants())
//        {
//            if (element.Parent?.Name != "Localization")
//                _resources[$"{element.Parent?.Name}.{element.Name}"] = element.Value;
//        }
//        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
//    }
//}

public class LocalizationManager : INotifyPropertyChanged
{
    public static LocalizationManager Instance { get; } = new LocalizationManager();
    private Dictionary<string, string> _resources = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public string this[string key] => _resources.TryGetValue(key, out var value) ? value : key;

    //public void LoadLanguage(CultureInfo culture)
    //{
    //    var filePath = $"i18n/MainModule.{culture.Name}.xml";
    //    Console.WriteLine("加载语言文件路径: " + filePath);

    //    var xdoc = XDocument.Load(filePath);

    //    _resources.Clear();
    //    //foreach (var element in xdoc.Descendants())
    //    //{
    //    //    var parent = element.Parent?.Name.LocalName ?? "(null)";
    //    //    if (parent != "Localization")
    //    //    {
    //    //        var key = $"{parent}.{element.Name.LocalName}";
    //    //        Console.WriteLine($"加载资源: {key} = {element.Value}");
    //    //        _resources[key] = element.Value;
    //    //    }
    //    //}
    //    foreach (var element in xdoc.Descendants())
    //    {
    //        if (element.Parent?.Name != "Localization")
    //            _resources[$"{element.Parent?.Name}.{element.Name}"] = element.Value;
    //    }
    //    // 通知绑定（见下一条）
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    //    //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Localized)));
    //}

    public void LoadLanguage(CultureInfo culture)
    {
        var filePath = $"i18n/MainModule.{culture.Name}.xml";
        Console.WriteLine("加载语言文件路径: " + filePath);

        var xdoc = XDocument.Load(filePath);

        _resources.Clear();
        int count = 0;
        foreach (var element in xdoc.Descendants())
        {
            if (element.Parent?.Name != "Localization")
            {
                var key = $"{element.Parent?.Name}.{element.Name}";
                Console.WriteLine($"加载资源: {key} = {element.Value}");
                _resources[key] = element.Value;
                count++;
            }
        }
        Console.WriteLine($"语言资源加载完成，共加载 {count} 项。");

        // 可选：打印几个关键 key 的内容
        Console.WriteLine($"示例 MainWindow.MenuItem1: {(_resources.TryGetValue("MainWindow.MenuItem1", out var v1) ? v1 : "(未找到)")}");
        Console.WriteLine($"示例 MainWindow.MenuItem2: {(_resources.TryGetValue("MainWindow.MenuItem2", out var v2) ? v2 : "(未找到)")}");

        // 通知绑定
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }

}

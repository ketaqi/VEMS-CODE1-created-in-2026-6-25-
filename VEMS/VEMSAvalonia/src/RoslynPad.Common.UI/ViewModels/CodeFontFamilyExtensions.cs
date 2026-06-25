using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RoslynPad.UI.MainViewModel;

namespace RoslynPad.UI.ViewModels
{
    public static class CodeFontFamilyExtensions
    {
        public static string ToFontName(this CodeFontFamily font)
        {
            return font switch
            {
                CodeFontFamily.Consolas => "Consolas",
                CodeFontFamily.JetBrainsMono => "JetBrains Mono",
                CodeFontFamily.MicrosoftYaHei => "Microsoft YaHei",
                CodeFontFamily.ComicSansMS => "Comic Sans MS",
                CodeFontFamily.Impact => "Impact",
                CodeFontFamily.FiraCode => "Fira Code",
                CodeFontFamily.CascadiaCode => "Cascadia Code",
                CodeFontFamily.SourceCodePro => "Source Code Pro",
                CodeFontFamily.Menlo => "Menlo",
                CodeFontFamily.Monaco => "Monaco",
                CodeFontFamily.CourierNew => "Courier New",
                CodeFontFamily.LiberationMono => "Liberation Mono",
                CodeFontFamily.UbuntuMono => "Ubuntu Mono",
                CodeFontFamily.DejaVuSansMono => "DejaVu Sans Mono",
                CodeFontFamily.SegoeUI => "Segoe UI",
                CodeFontFamily.Arial => "Arial",
                CodeFontFamily.TimesNewRoman => "Times New Roman",
                CodeFontFamily.Tahoma => "Tahoma",
                CodeFontFamily.Verdana => "Verdana",
                CodeFontFamily.LucidaConsole => "Lucida Console",
                CodeFontFamily.SFMono => "SF Mono",
                CodeFontFamily.NotoSansMono => "Noto Sans Mono",
                CodeFontFamily.DroidSansMono => "Droid Sans Mono",
                _ => "Consolas"
            };
        }
    }

}

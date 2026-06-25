using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using System;

namespace OpticalChainSimulator.Services.Theme;

/// <summary>
/// 应用主题类型枚举
/// 定义应用支持的明暗两种主题模式
/// </summary>
public enum AppTheme
{
    /// <summary>
    /// 深色主题
    /// </summary>
    Dark,
    /// <summary>
    /// 浅色主题
    /// </summary>
    Light
}

/// <summary>
/// 应用主题管理类
/// 负责应用主题的切换、应用和状态管理，基于Avalonia的ThemeVariant实现
/// </summary>
public static class ThemeManager
{
    /// <summary>
    /// 私有字段：当前激活的应用主题
    /// 默认值为深色主题（Dark）
    /// </summary>
    private static AppTheme _currentTheme = AppTheme.Dark;

    /// <summary>
    /// 公开属性：获取或设置当前应用主题
    /// 设置时会自动检测值是否变化，若变化则更新并应用新主题
    /// </summary>
    public static AppTheme CurrentTheme
    {
        get => _currentTheme;
        set
        {
            // 仅当新值与当前值不同时，才执行主题更新逻辑
            if (_currentTheme != value)
            {
                _currentTheme = value;
                // 应用新选择的主题
                ApplyTheme(value);
            }
        }
    }

    /// <summary>
    /// 将指定的主题应用到当前应用程序
    /// </summary>
    /// <param name="theme">需要应用的主题类型（Dark/Light）</param>
    public static void ApplyTheme(AppTheme theme)
    {
        // 获取当前Avalonia应用程序实例
        var app = Application.Current;
        // 若应用实例为空，直接返回（避免空引用异常）
        if (app == null) return;

        // 根据传入的主题类型，设置Avalonia应用的RequestedThemeVariant
        app.RequestedThemeVariant = theme switch
        {
            AppTheme.Light => ThemeVariant.Light, // 浅色主题
            _ => ThemeVariant.Dark                // 默认（包括Dark）为深色主题
        };
    }

    /// <summary>
    /// 切换应用主题（明暗主题互切）
    /// 若当前为深色则切换为浅色，反之则切换为深色
    /// </summary>
    public static void ToggleTheme()
    {
        CurrentTheme = CurrentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
    }
}
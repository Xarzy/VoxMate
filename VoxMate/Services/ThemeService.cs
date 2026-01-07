using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;

namespace VoxMate.Services
{
    public static class ThemeService
    {
        public static void Initialize()
        {
            var app = Application.Current;
            if (app == null)
                return;

            ApplyTheme(app.RequestedTheme);
            app.RequestedThemeChanged += OnRequestedThemeChanged;
        }

        private static void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
        {
            ApplyTheme(e.RequestedTheme);
        }

        private static void ApplyTheme(AppTheme theme)
        {
            var resources = Application.Current?.Resources;
            if (resources == null)
                return;

            bool isLight = theme == AppTheme.Light;

            resources["PageBackgroundColor"] = isLight ? Color.FromArgb("#F0F6FF") : Color.FromArgb("#071022");
            resources["CardBackground"] = isLight ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#0D1A2B");
            resources["AccentColor"] = isLight ? Color.FromArgb("#5B8CFF") : Color.FromArgb("#6EA0FF");
            resources["SectionTitleColor"] = isLight ? Color.FromArgb("#6B7280") : Color.FromArgb("#9AA6BF");
            resources["PrimaryTextColor"] = isLight ? Color.FromArgb("#0B1220") : Color.FromArgb("#E6F0FF");
            resources["SecondaryTextColor"] = isLight ? Color.FromArgb("#1F2937") : Color.FromArgb("#D8E7FF");
            resources["HistoryItemBackground"] = isLight ? Color.FromArgb("#F5F7FF") : Color.FromArgb("#072033");
        }
    }
}
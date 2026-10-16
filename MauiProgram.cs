using Microsoft.Extensions.Logging;
using UtauTtsBuilder.Services;

namespace UtauTtsBuilder;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // ===== 全局异常捕获 =====
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var ex = e.ExceptionObject as Exception
                ?? new Exception(e.ExceptionObject?.ToString() ?? "unknown");
            Logger.Error("AppDomain", ex);
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Logger.Error("Task", e.Exception);
            e.SetObserved();
        };

#if ANDROID
        // Entry / Picker 去下划线
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(
            "RemoveUnderline", (handler, view) =>
            {
                handler.PlatformView.BackgroundTintList =
                    global::Android.Content.Res.ColorStateList.ValueOf(
                        global::Android.Graphics.Color.Transparent);
            });

        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping(
            "RemoveUnderline", (handler, view) =>
            {
                handler.PlatformView.BackgroundTintList =
                    global::Android.Content.Res.ColorStateList.ValueOf(
                        global::Android.Graphics.Color.Transparent);
            });

        Microsoft.Maui.Handlers.SwitchHandler.Mapper.AppendToMapping(
            "UnifiedSwitch", (handler, view) =>
            {
                handler.PlatformView.ThumbTintList =
                    global::Android.Content.Res.ColorStateList.ValueOf(
                        global::Android.Graphics.Color.White);
                handler.PlatformView.BackgroundTintList =
                    global::Android.Content.Res.ColorStateList.ValueOf(
                        global::Android.Graphics.Color.Transparent);
            });

        Microsoft.Maui.Handlers.RadioButtonHandler.Mapper.AppendToMapping(
            "RemoveBackground", (handler, view) =>
            {
                handler.PlatformView.BackgroundTintList =
                    global::Android.Content.Res.ColorStateList.ValueOf(
                        global::Android.Graphics.Color.Transparent);
            });
#endif

        return builder.Build();
    }
}

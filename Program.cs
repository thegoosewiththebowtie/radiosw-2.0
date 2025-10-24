global using static radiosw.ERROR;
namespace radiosw {
    using System;
    using System.Globalization;
    using System.IO;
    using Avalonia;
    using Avalonia.ReactiveUI;

    internal sealed class Program {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [ STAThread ]
        public static void Main(String[] arg_args) => Program.BuildAvaloniaApp().StartWithClassicDesktopLifetime(arg_args);

        // Avalonia configuration, don't remove; also used by visual designer.
        private static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace().UseReactiveUI();
    }
}
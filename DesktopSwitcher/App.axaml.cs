using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using DesktopSwitcher.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;

namespace DesktopSwitcher;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }


    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var viewModel = new MainViewModel(desktop);
            DataContext = viewModel;

            viewModel.UpdateDesktopFolderMenu();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

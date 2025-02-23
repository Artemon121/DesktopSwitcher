using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DesktopSwitcher.Models;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;
using Tomlyn;

namespace DesktopSwitcher.ViewModels;

public class MainViewModel : ViewModelBase
{
    IClassicDesktopStyleApplicationLifetime desktopLifetime;

    public Config Config = new();

    public MainViewModel(IClassicDesktopStyleApplicationLifetime desktopLifetime)
    {
        this.desktopLifetime = desktopLifetime;
        LoadConfig();
    }

    public void Exit() => desktopLifetime.Shutdown();

    public void UpdateDesktopFolderMenu()
    {
        var trayIcons = TrayIcon.GetIcons(App.Current!)!;
        if (trayIcons.Count > 0)
        {
            var trayIcon = trayIcons[0];
            var desktopsMenuItem = trayIcon.Menu?.Items.OfType<NativeMenuItem>()
                .FirstOrDefault(item => item.Header?.ToString() == "Desktops");

            if (desktopsMenuItem?.Menu != null)
            {
                desktopsMenuItem.Menu.Items.Clear();

                foreach (var folder in Config.DesktopFolders)
                {
                    var menuItem = new NativeMenuItem { Header = folder.Name };
                    menuItem.Click += (s, e) => SwitchDesktop(folder);
                    desktopsMenuItem.Menu.Add(menuItem);
                }
            }
        }
    }

    private void SetRunAtStartup(bool enabled) { 
        var reg = new RegistryKeyValue
        {
            KeyPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run",
            ValueName = "DesktopSwitcher",
            ValueKind = RegistryValueKind.String,
            Value = Environment.ProcessPath
        };
        if (enabled)
        {
            RegistryManager.SetKeyValue(reg);
        } else
        {
            RegistryManager.DeleteKeyValue(reg.KeyPath, reg.ValueName);
        }
    }

    public void LoadConfig()
    {
        try
        {
            var configPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "config.toml");
            var configText = File.ReadAllText(configPath);
            Config = Toml.ToModel<Config>(configText);
            Config.Path = configPath;
            SetRunAtStartup(Config.RunAtStartup);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading config: {ex.Message}");
        }
    }


    private void SaveConfig()
    {
        try
        {
            var configText = Toml.FromModel(Config);
            File.WriteAllText(Config.Path, configText);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving config: {ex.Message}");
        }
    }

    public void ReloadConfig()
    {
        LoadConfig();
        UpdateDesktopFolderMenu();
    }

    public void OpenConfigFile()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = Config.Path,
            UseShellExecute = true
        });
    }

    private DesktopFolder? GetCurrentDesktopFolder()
    {
        var desktopReg = RegistryManager.GetKeyValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", "Desktop");

        var expandedDesktopPath = Environment.ExpandEnvironmentVariables((string)desktopReg.Value);
        var desktopPath = Path.GetFullPath(expandedDesktopPath);

        return Config.DesktopFolders.Find(folder => Path.GetFullPath(Environment.ExpandEnvironmentVariables(folder.Path)) == desktopPath);
    }

    private void SaveIconLayout(DesktopFolder toFolder)
    {
        var iconSizeReg = RegistryManager.GetKeyValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\Shell\Bags\1\Desktop", "IconSize");
        toFolder.IconSize = (int)iconSizeReg?.Value;
        var iconLayoutsReg = RegistryManager.GetKeyValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\Shell\Bags\1\Desktop", "IconLayouts");
        var iconLayoutsBin = (byte[])iconLayoutsReg?.Value;
        toFolder.IconLayouts = Convert.ToBase64String(iconLayoutsBin);
    }

    private void LoadIconLayout(DesktopFolder fromFolder)
    {
        if (fromFolder.IconLayouts is null || fromFolder.IconSize == 0) return;

        RegistryKeyValue iconSizeReg = new()
        {
            KeyPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\Shell\Bags\1\Desktop",
            ValueName = "IconSize",
            ValueKind = RegistryValueKind.DWord,
            Value = fromFolder.IconSize
        };
        RegistryManager.SetKeyValue(iconSizeReg);

        RegistryKeyValue iconLayoutsReg = new()
        {
            KeyPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\Shell\Bags\1\Desktop",
            ValueName = "IconLayouts",
            ValueKind = RegistryValueKind.Binary,
            Value = Convert.FromBase64String(fromFolder.IconLayouts)
        };
        RegistryManager.SetKeyValue(iconLayoutsReg);
    }

    public void SwitchDesktop(DesktopFolder folder)
    {
        // Create desktop folder if it doesn't exist
        if (!Directory.Exists(folder.Path))
        {
            Directory.CreateDirectory(folder.Path);
        }

        // Refresh explorer so it stores icon layout data to the registry
        ExplorerUtils.Refresh();
        Thread.Sleep(100);

        // Save current layout and config
        var current = GetCurrentDesktopFolder();
        SaveIconLayout(current);
        SaveConfig();

        // Kill Explorer (we use taskkill so it doesn't come back automatically)
        ExplorerUtils.TaskKill();

        // Set registry keys for new desktop folder
        var registryKeyValue = new RegistryKeyValue()
        {
            KeyPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders",
            ValueName = "Desktop",
            ValueKind = RegistryValueKind.ExpandString,
            Value = folder.Path,
        };
        RegistryManager.SetKeyValue(registryKeyValue);

        LoadIconLayout(folder);

        // Wait a bit and start explorer back
        Thread.Sleep(250);
        Process.Start("explorer.exe");
    }
}

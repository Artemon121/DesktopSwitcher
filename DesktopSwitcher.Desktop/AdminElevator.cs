using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

public static class AdminElevator
{
    public static bool EnsureAdminPrivileges()
    {
        if (!IsRunningAsAdmin())
        {
            RestartAsAdmin();
            return false;
        }
        return true;
    }

    private static bool IsRunningAsAdmin()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static void RestartAsAdmin()
    {
        var exePath = Environment.ProcessPath;
        var startInfo = new ProcessStartInfo(exePath)
        {
            UseShellExecute = true,
            Verb = "runas"
        };

        try
        {
            Process.Start(startInfo);
            Environment.Exit(0);
        }
        catch
        {
            // User cancelled UAC prompt
            Environment.Exit(1);
        }
    }
}
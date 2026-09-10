using System.Diagnostics;

static class ShutdownCommand
{
    public const int Success = 0;

    public static int Schedule(int seconds)
    {
        return Run("/s /f /t " + seconds);
    }

    public static int Cancel()
    {
        return Run("/a");
    }

    static int Run(string arguments)
    {
        ProcessStartInfo info = new ProcessStartInfo("shutdown.exe", arguments);
        info.UseShellExecute = false;
        info.CreateNoWindow = true;
        using (Process process = Process.Start(info))
        {
            process.WaitForExit();
            return process.ExitCode;
        }
    }
}

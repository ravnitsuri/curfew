using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Mutex = System.Threading.Mutex;

static class Program
{
    [DllImport("user32.dll")]
    static extern bool PostMessage(IntPtr window, int message, IntPtr wparam, IntPtr lparam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int RegisterWindowMessage(string name);

    [DllImport("user32.dll")]
    static extern bool AllowSetForegroundWindow(int processId);

    const string InstanceLockName = @"Local\CurfewSingleInstance";
    const int AnyProcess = -1;
    static readonly IntPtr AllWindows = (IntPtr)0xFFFF;

    public static readonly int ShowWindowMessage = RegisterWindowMessage("CurfewShowWindow");

    static Mutex instanceLock;

    [STAThread]
    static void Main()
    {
        bool firstInstance;
        instanceLock = new Mutex(true, InstanceLockName, out firstInstance);

        if (!firstInstance)
        {
            AllowSetForegroundWindow(AnyProcess);
            PostMessage(AllWindows, ShowWindowMessage, IntPtr.Zero, IntPtr.Zero);
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainWindow());
        GC.KeepAlive(instanceLock);
    }
}

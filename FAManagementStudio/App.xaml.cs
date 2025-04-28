using FAManagementStudio.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace FAManagementStudio
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            AppSettingsManager.Save();
            //FB 3秒問題のため、暫定実装。
            UnloadDll(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), @"fb25\fbembed.dll"));
            base.OnExit(e);
        }
        [DllImport("kernel32", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);
        private static void UnloadDll(string path)
        {
            foreach (ProcessModule dll in Process.GetCurrentProcess().Modules)
            {
                if (dll.FileName.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    FreeLibrary(dll.BaseAddress);
                }
            }
        }
    }
}

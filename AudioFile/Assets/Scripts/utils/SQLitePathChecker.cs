using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AudioFile.Utilities
{
    public class SQLitePathChecker : MonoBehaviour
    {
        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        void Start()
        {
            string[] possiblePaths = {
            @"C:\Program Files\Amazon\AWSCLIV2\sqlite3.dll",
            @"C:\Users\natha\anaconda3\Library\bin\sqlite3.dll",
            Path.Combine(Application.dataPath, "Plugins", "SQLite", "sqlite3.dll")
        };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    IntPtr handle = GetModuleHandle("sqlite3.dll");
                    if (handle != IntPtr.Zero)
                    {
                        Debug.Log($"Unity is using: {path}");
                        return;
                    }
                }
            }

            Debug.LogError("Unity is using an unknown `sqlite3.dll`.");
        }
    }
}


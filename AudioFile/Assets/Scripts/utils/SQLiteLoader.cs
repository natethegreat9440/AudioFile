using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AudioFile.Utilities
{
    public class SQLiteLoader
    {
        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        public static Assembly LoadedSQLiteAssembly { get; private set; }

        public static void ForceLoadLocalSQLite()
        {
            string dllPath;
            string pluginPath;
            string currentPath = Environment.GetEnvironmentVariable("PATH");
            
            if (Application.isEditor)
            {
                pluginPath = Path.Combine(Application.dataPath, "Plugins", "SQLite");
                Environment.SetEnvironmentVariable("PATH", pluginPath + ";" + currentPath);

                //dllPath = Path.Combine(Application.dataPath, "Plugins", "SQLite", "Mono.Data.Sqlite.dll");
                dllPath = Path.Combine(Application.dataPath, "Plugins", "SQLite", "sqlite3.dll");

            }
            else
            {
                pluginPath = Path.Combine(Application.dataPath, "Managed");
                Environment.SetEnvironmentVariable("PATH", pluginPath + ";" + currentPath);

                //dllPath = Path.Combine(Application.dataPath, "Managed", "Mono.Data.Sqlite.dll"); // Adjust for build
                dllPath = Path.Combine(Application.dataPath, "Managed", "sqlite3.dll"); // Adjust for build
            }

            if (File.Exists(dllPath))
            {
                IntPtr handle = LoadLibrary(dllPath);
                if (handle == IntPtr.Zero)
                {
                    Debug.LogError("Failed to load local SQLite3 DLL.");
                }
                else
                {
                    Debug.Log("Successfully loaded local SQLite3 DLL.");
                }

                /*try //This causes errors and isn't needed. Subject for deletion
                {
                    // **Explicitly load the assembly**
                    LoadedSQLiteAssembly = Assembly.LoadFile(dllPath);
                    Debug.Log($"Loaded SQLite Assembly: {LoadedSQLiteAssembly.FullName}");
                }
                catch (Exception ex)
                {
                    //Debug.LogError($"Error loading Mono.Data.Sqlite assembly: {ex.Message}");
                    Debug.LogError($"Error loading sqlite3 assembly: {ex.Message}");
                }*/
            }
            else
            {
                Debug.LogError($"Local SQLite3 DLL not found at: {dllPath}");
            }
        }
    }
}


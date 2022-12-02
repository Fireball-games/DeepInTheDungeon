using System.IO;
using UnityEngine;

namespace Scripts.Helpers
{
    public static class FileOperationsHelper
    {
        public const string MapDirectory = "/Maps";
        
        private static readonly string ApplicationPath = Application.persistentDataPath;

        public static string[] GetFilesInDirectory(string relativeDirectoryPath = "")
        {
            string fullPath = Path.Combine(ApplicationPath, relativeDirectoryPath);
            return !Directory.Exists(fullPath) ? null : Directory.GetFiles(fullPath);
        }
    }
}
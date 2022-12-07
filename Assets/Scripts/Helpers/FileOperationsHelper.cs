using System.IO;
using System.Linq;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace Scripts.Helpers
{
    public static class FileOperationsHelper
    {
        public const string MapDirectory = "Maps";
        
        private static readonly string ApplicationPath = Application.persistentDataPath;

        public static string[] GetFilesInDirectory(string relativeDirectoryPath = "", string extensionFilter = "all")
        {
            string fullPath = Path.Combine(ApplicationPath, relativeDirectoryPath);

            string[] allFiles = !Directory.Exists(fullPath) ? null : Directory.GetFiles(fullPath);

            if (allFiles == null) return null;

            if (extensionFilter != "all")
            {
                allFiles = allFiles.Where(file => Path.GetExtension(file) == extensionFilter).ToArray();
            }

            return allFiles;
        }

        public static string GetSavePath(string mapName)
        {
            return Path.Combine(MapDirectory, $"{mapName}.map");
        }
    }
}
using Synchronizer.Log;
using System;
using System.IO;

namespace Synchronizer.Processing
{
    public class FSActions
    {
        public static Boolean TryCreateDirectory(String path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                Logger.RaiseMessage("Create Directory Error: " + ex.Message + "\n" + path);
                return false;
            }

            return true;
        }

        public static Boolean TryCopyFile(String sourcePath, String destPath, Boolean rewrite)
        {
            try
            {
                File.Copy(sourcePath, destPath, rewrite);
            }
            catch (Exception ex)
            {
                Logger.RaiseMessage("Copy File Error: " + ex.Message + "\n" + destPath);
                return false;
            }

            return true;
        }

        public static Boolean TryDeleteDirectory(String path)
        {
            try
            {
                Directory.Delete(path);
            }
            catch (Exception ex)
            {
                Logger.RaiseMessage("Delete Directory Error: " + ex.Message + "\n" + path);
                return false;
            }

            return true;
        }

        public static Boolean TryDeleteFile(String path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                Logger.RaiseMessage("Delete File Error: " + ex.Message + "\n" + path);
                return false;
            }

            return true;
        }

        public static void SaveFileToTemp(String path, String rootPath, String tempRootPath)
        {
            String rootTmpPath = tempRootPath;
            String subDirs = path.Substring(rootPath.Length + 1);
            String tmpPath = Path.Combine(rootTmpPath, subDirs);

            String[] items = subDirs.Split('\\');
            for (Int32 i = 0; i < items.Length - 1; i++)
            {
                rootTmpPath = Path.Combine(rootTmpPath, items[i]);
                if (!Directory.Exists(rootTmpPath))
                    if (!TryCreateDirectory(rootTmpPath))
                        throw new Exception("Can't create directory\n" + rootTmpPath);
            }
            if (!TryCopyFile(path, tmpPath, false))
                throw new Exception("Can't save file to temp\n");
        }

        public static String ConvertPath(String path, String fromRootPath, String toRootPath)
        {
            return Path.Combine(toRootPath, path.Substring(fromRootPath.Length + 1));
        }
    }
}

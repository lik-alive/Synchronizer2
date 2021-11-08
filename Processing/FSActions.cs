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
                Logger.RaiseError("Create Directory Error: " + ex.Message + "\n" + path);
                return false;
            }

            return true;
        }

        public static Boolean TryCopyFile(String sourcePath, String destPath, Boolean overwrite = true)
        {
            try
            {
                File.Copy(sourcePath, destPath, overwrite);
            }
            catch (Exception ex)
            {
                Logger.RaiseError("Copy File Error: " + ex.Message + "\n" + destPath);
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
                Logger.RaiseError("Delete Directory Error: " + ex.Message + "\n" + path);
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
                Logger.RaiseError("Delete File Error: " + ex.Message + "\n" + path);
                return false;
            }

            return true;
        }

        public static String ConvertPath(String path, String fromRootPath, String toRootPath)
        {
            return Path.Combine(toRootPath, path.Substring(fromRootPath.Length + 1));
        }
    }
}

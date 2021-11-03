using System;
using System.IO;
using System.Threading;

namespace Synchronizer.Processing
{
    /*public class FSSyncher : AsyncBase
    {
        private Boolean SAVE_MODE = true;

        private String tempSaveFolder;

        private CmpDirectory iroot1, iroot2;
        
        public FSSyncher(CmpDirectory iroot1, CmpDirectory iroot2, Boolean deleteDuplicates = false)
        {
            this.iroot1 = iroot1;
            this.iroot2 = iroot2;

            execThread = new Thread(() =>
            {
                try
                {
                    if (SAVE_MODE)
                    {
                        tempSaveFolder = Path.Combine(Path.GetTempPath(), "MySync\\ManualSaved");

                        if (!Directory.Exists(tempSaveFolder))
                            if (!FSActions.TryCreateDirectory(tempSaveFolder))
                                return;

                        tempSaveFolder = Path.Combine(tempSaveFolder, DateTime.Now.ToString("H_mm_ss yyyy_MM_dd"));
                        while (Directory.Exists(tempSaveFolder))
                            tempSaveFolder += " 1";

                        if (!FSActions.TryCreateDirectory(tempSaveFolder)) return;
                    }

                    RecursiveCalcSizeCopy(iroot1);
                    RecursiveSync(iroot1, iroot1.FullName, iroot2.FullName);

                    if (deleteDuplicates)
                    {
                        RecursieveCalcSizeDelete(iroot2);
                        RecursiveDelete(iroot2, iroot2.FullName);
                    }
                }
                catch (Exception ex)
                {
                    Logger.RaiseMessage("Error: " + ex.Message);
                }
            });
        }

        private Double SizeCopy = 0, SizeDelete = 0;
        
        private void RecursiveCalcSizeCopy(CmpDirectory root)
        {
            foreach (var dir in root.Directories)
            {
                if (forceStop) return;

                if (!dir.IsSyncing) continue;

                RecursiveCalcSizeCopy(dir);
            }

            foreach (var file in root.Files)
            {
                if (forceStop) return;

                if (!file.IsSyncing) continue;

                SizeCopy += file.FSFile.Length;
            }
        }

        private void RecursieveCalcSizeDelete(CmpDirectory root)
        {
            foreach (var dir in root.Directories)
            {
                if (forceStop) return;

                if (!dir.IsSyncing) continue;

                RecursieveCalcSizeDelete(dir);
            }

            foreach (var file in root.Files)
            {
                if (forceStop) return;

                if (!file.IsSyncing) continue;

                SizeDelete += file.FSFile.Length;
            }
        }

        private void RecursiveSync(CmpDirectory root, String fromRootPath, String toRootPath)
        {
            foreach (var dir in root.Directories)
            {
                if (forceStop) return;

                if (!dir.IsSyncing) continue;

                String newPath = FSActions.ConvertPath(dir.FullName, fromRootPath, toRootPath);

                if (dir.IsUnique)
                    if (!FSActions.TryCreateDirectory(newPath))
                        continue;

                RecursiveSync(dir, fromRootPath, toRootPath);
            }

            foreach (var file in root.Files)
            {
                if (forceStop) return;

                if (!file.IsSyncing) continue;

                String newPath = FSActions.ConvertPath(file.FullName, fromRootPath, toRootPath);

                if (file.IsUnique) FSActions.TryCopyFile(file.FullName, newPath, false);
                else
                {
                    if (SAVE_MODE)
                        FSActions.SaveFileToTemp(newPath, toRootPath, tempSaveFolder);
                    FSActions.TryCopyFile(file.FullName, newPath, true);
                }
                Progress += file.FSFile.Length / SizeCopy;
            }
        }

        private void RecursiveDelete(CmpDirectory root, String fromRootPath)
        {
            foreach (var file in root.Files)
            {
                if (forceStop) return;

                if (!file.IsUnique || !file.IsSyncing) continue;

                if (SAVE_MODE)
                    FSActions.SaveFileToTemp(file.FullName, fromRootPath, tempSaveFolder);
                FSActions.TryDeleteFile(file.FullName);

                Progress += file.FSFile.Length / SizeDelete;
            }

            foreach (var dir in root.Directories)
            {
                if (forceStop) return;

                if (!dir.NestedFlags[0] || !dir.IsSyncing) continue;

                RecursiveDelete(dir, fromRootPath);

                if (dir.IsUnique) FSActions.TryDeleteDirectory(dir.FullName);
            }
        }
    }*/
}

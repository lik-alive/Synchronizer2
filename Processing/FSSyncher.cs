using Synchronizer2.Log;
using Synchronizer2.Model;
using System;
using System.IO;
using System.Threading;

namespace Synchronizer2.Processing
{
    public class FSSyncher : AsyncBase
    {
        public FSSyncher(FSTree tree1, FSTree tree2, Boolean deleteUnique = false)
        {
            execThread = new Thread(() =>
            {
                DateTime start = DateTime.Now;
                Logger.RaiseLog("Synchronization in progress...");

                // Calc files count
                long copySize = RecursiveCalcCopySize(tree1.Root);

                if (deleteUnique)
                {
                    Int32 deleteCount = RecursiveCalcDeleteCount(tree2.Root);
                    syncProgressInc = 90.0 / copySize;
                    deleteProgressInc = 10.0 / deleteCount;
                }
                else
                {
                    syncProgressInc = 100.0 / copySize;
                }

                // Synchronize
                fromRootPath = tree1.FullName;
                toRootPath = tree2.FullName;
                RecursiveSync(tree1.Root);

                // Delete unique
                if (deleteUnique) RecursiveDelete(tree2.Root);

                if (forceStop)
                {
                    Logger.RaiseError("Synchronization stoped");
                }
                else
                {
                    if (errorsCount > 0)
                    {
                        Logger.RaiseError("Synchronization completed with errors (" + (DateTime.Now - start).TotalSeconds + " s)");
                        forceStop = true;
                    }
                    else
                    {
                        Logger.RaiseLog("Synchronization completed (" + (DateTime.Now - start).TotalSeconds + " s)");
                    }
                }
            });
        }

        private Double syncProgressInc, deleteProgressInc;

        private String fromRootPath, toRootPath;

        private Int32 errorsCount = 0;

        private long RecursiveCalcCopySize(FSDirectory root)
        {
            long count = 0;

            foreach (FSItem item in root.UnequalChildren)
            {
                if (forceStop) return -1;

                if (item.IsChecked != false)
                {
                    if (item.IsDirectory) count += RecursiveCalcCopySize(item as FSDirectory);
                    else count += (item as FSFile).Length;
                }
            }

            return count;
        }

        private Int32 RecursiveCalcDeleteCount(FSDirectory root)
        {
            Int32 count = 0;

            foreach (FSItem item in root.UnequalChildren)
            {
                if (forceStop) return -1;

                if (item.IsChecked == true)
                {
                    if (item.IsDirectory) count += RecursiveCalcDeleteCount(item as FSDirectory);
                    else count += root.IsUnique ? 1 : 0;
                }
            }

            return count;
        }

        private void RecursiveSync(FSDirectory root)
        {
            foreach (FSItem item in root.UnequalChildren)
            {
                if (forceStop) return;

                // Skip unselected
                if (item.IsChecked == false) continue;

                String newPath = FSActions.ConvertPath(item.FullName, fromRootPath, toRootPath);

                // Create directory
                if (item.IsDirectory)
                {
                    if (item.IsUnique)
                    {
                        if (!FSActions.TryCreateDirectory(newPath))
                        {
                            errorsCount++;
                            continue;
                        }
                    }
                    RecursiveSync(item as FSDirectory);
                }
                // Copy file
                else
                {
                    if (item.Twin != null) FSActions.TryDeleteFile(newPath);
                    TryCopyFileStreamed(item.FullName, newPath);
                }
            }
        }

        private void RecursiveDelete(FSDirectory root)
        {
            foreach (FSItem item in root.UnequalChildren)
            {
                if (forceStop) return;

                // Skip unselected
                if (item.IsChecked == false) continue;

                // Delete directory
                if (item.IsDirectory)
                {
                    RecursiveDelete(item as FSDirectory);

                    if (!item.IsUnique) continue;
                    if (!FSActions.TryDeleteDirectory(item.FullName)) errorsCount++;
                }
                // Delete file
                else
                {
                    // Skip non-unique
                    if (!item.IsUnique) continue;
                    if (!FSActions.TryDeleteFile(item.FullName)) errorsCount++;
                    Progress += deleteProgressInc;
                }
            }
        }

        private void TryCopyFileStreamed(String sourcePath, String destPath)
        {
            byte[] buffer = new byte[4 * 1024];
            int c;
            try
            {
                using (FileStream fr = File.OpenRead(sourcePath))
                {
                    using (FileStream fw = File.OpenWrite(destPath))
                    {
                        while ((c = fr.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            if (forceStop)
                            {
                                fw.Close();
                                FSActions.TryDeleteFile(destPath);
                                return;
                            }

                            fw.Write(buffer, 0, c);
                            Progress += syncProgressInc * c;
                        }
                    }
                }

                // Set flags
                File.SetCreationTime(destPath, File.GetCreationTime(sourcePath));
                File.SetLastAccessTime(destPath, File.GetLastAccessTime(sourcePath));
                File.SetLastWriteTime(destPath, File.GetLastWriteTime(sourcePath));
            }
            catch (Exception ex)
            {
                errorsCount++;
                Logger.RaiseError("Copy File Error: " + ex.Message + "\n" + destPath);
            }
        }
    }
}

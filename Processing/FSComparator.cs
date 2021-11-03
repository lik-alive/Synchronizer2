using Synchronizer.Model;
using System;
using System.Linq;
using System.Threading;


namespace Synchronizer.Processing
{
    public class FSComparator : AsyncBase
    {
        public FSDirectory Root1 { get; private set; }

        public FSDirectory Root2 { get; private set; }

        public FSComparator(FSDirectory root1, FSDirectory root2)
        {
            Root1 = root1;
            Root2 = root2;

            execThread = new Thread(() =>
            {
                RecursiveComparison(Root1, Root2, 100);
                //TODO remove
                RecursiveUpdateFlags(Root1);
                RecursiveUpdateFlags(Root2);
            });
        }

        private void RecursiveComparison(FSDirectory root1, FSDirectory root2, Double progressInc)
        {
            Double progressIncChild = progressInc / root1.Children.Count;

            // Process directories
            for (Int32 i = root1.Children.Count - 1; i >= 0; i--)
            {
                if (forceStop) return;
                if (!root1.Children[i].IsDirectory) continue;

                FSItem dir1 = root1.Children[i];

                // Search for a twin
                foreach (FSItem dir2 in root2.Children)
                {
                    if (forceStop) return;
                    if (!root1.Children[i].IsDirectory) continue;

                    if (dir1.LowerName == dir2.LowerName)
                    {
                        FSItem.SetTwins(dir1, dir2);
                        RecursiveComparison(dir1 as FSDirectory, dir2 as FSDirectory, progressIncChild);
                        break;
                    }
                }
            }

            // Process files
            for (Int32 i = root1.Children.Count - 1; i >= 0; i--)
            {
                if (forceStop) return;
                if (root1.Children[i].IsDirectory) continue;

                FSItem file1 = root1.Children[i];

                // Search for a twin
                foreach (FSItem file2 in root2.Children)
                {
                    if (forceStop) return;
                    if (root1.Children[i].IsDirectory) continue;

                    if (file1.LowerName == file2.LowerName)
                    {
                        FSItem.SetTwins(file1, file2);

                        Boolean sameDate = (file1 as FSFile).LastWriteTime == (file2 as FSFile).LastWriteTime;
                        Boolean sameSize = (file1 as FSFile).Length == (file2 as FSFile).Length;

                        // Set flags for different files
                        if (!sameDate || !sameSize)
                        {
                            FSFile.SetTwinFlags(file1 as FSFile, file2 as FSFile);
                        }
                        // Remove equal files from the list
                        else
                        {
                            file1.Disembed();
                            file2.Disembed();
                        }
                        Progress += progressIncChild;

                        break;
                    }
                }
            }

            Progress += progressIncChild;
        }

        /// <summary>
        /// Проброс флагов наверх к корню
        /// </summary>
        /// <param name="root"></param>
        private void RecursiveUpdateFlags(FSDirectory root)
        {
            foreach (FSItem item in root.Children)
            {
                if (forceStop) return;
                if (!item.IsDirectory) continue;

                RecursiveUpdateFlags(item as FSDirectory);

                item.UpdateFlagsToRoot();
            }

            foreach (FSItem item in root.Children)
            {
                if (forceStop) return;
                if (item.IsDirectory) continue;

                item.UpdateFlagsToRoot();
            }
        }
    }
}

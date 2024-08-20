using Synchronizer2.Log;
using Synchronizer2.Model;
using System;
using System.Threading;


namespace Synchronizer2.Processing
{
    public class FSAnalyzer : AsyncBase
    {
        public FSAnalyzer(FSTree tree1, FSTree tree2)
        {
            execThread = new Thread(() =>
            {
                DateTime start = DateTime.Now;
                Logger.RaiseLog("Analysis in progress...");

                // Clear all flags
                tree1.Root.Clear();
                tree2.Root.Clear();

                // Calc root1 items count
                progressInc = 100.0 / RecursiveCalcCount(tree1.Root);

                // Analysis
                RecursiveAnalysis(tree1.Root, tree2.Root);

                // Set check statuses (DO NOT REFACTOR THIS, NAIVE ONE - YOU WON'T COVER ALL CASES!)
                SetCheckStatuses(tree1.Root);
                SetCheckStatuses(tree2.Root);

                if (forceStop)
                {
                    Logger.RaiseError("Analysis stoped");
                }
                else
                {
                    Logger.RaiseLog("Analysis completed (" + (DateTime.Now - start).TotalSeconds + " s)");
                    tree1.IsAnalyzed = true;
                    tree2.IsAnalyzed = true;
                }
            });
        }

        private Double progressInc;

        private Int32 RecursiveCalcCount(FSDirectory item)
        {
            Int32 count = 1;
            foreach (FSItem child in item.Children)
            {
                if (child.IsDirectory)
                {
                    count += RecursiveCalcCount(child as FSDirectory);
                } else
                {
                    count++;
                }
                
            }
            return count;
        }

        private void RecursiveAnalysis(FSDirectory root1, FSDirectory root2)
        {
            // Compare items
            foreach (FSItem item1 in root1.Children)
            {
                if (forceStop) return;
                Progress += progressInc;

                // Search for a twin
                foreach (FSItem item2 in root2.Children)
                {
                    if (forceStop) return;

                    if (item1.LowerName == item2.LowerName)
                    {
                        FSItem.SetTwins(item1, item2);

                        if (item1.IsDirectory)
                        {
                            RecursiveAnalysis(item1 as FSDirectory, item2 as FSDirectory);
                        }
                        else
                        {
                            CompareFiles(item1, item2);
                        }
                        break;
                    }
                }
            }

            // Set nested flags
            Boolean equal1 = true;
            foreach (FSItem item1 in root1.Children)
            {
                equal1 &= item1.IsEqual;
                root1.NestedFlags[0] |= item1.NestedFlags[0] | item1.IsUnique;
                for (Int32 i = 1; i < 5; i++)
                    root1.NestedFlags[i] |= item1.NestedFlags[i];
            }

            Boolean equal2 = true;
            foreach (FSItem item2 in root2.Children)
            {
                equal2 &= item2.IsEqual;
                root2.NestedFlags[0] |= item2.NestedFlags[0] | item2.IsUnique;
                for (Int32 i = 1; i < 5; i++)
                    root2.NestedFlags[i] |= item2.NestedFlags[i];
            }

            if (equal1 && equal2) FSItem.SetEquals(root1, root2);
        }

        /// <summary>
        /// Сравнение двух файлов
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        private void CompareFiles(FSItem item1, FSItem item2)
        {
            FSFile file1 = item1 as FSFile;
            FSFile file2 = item2 as FSFile;

            Boolean sameDate = Math.Abs((file1.LastWriteTime - file2.LastWriteTime).TotalSeconds) <= 5;
            Boolean sameSize = file1.Length == file2.Length;

            
            if (sameDate && sameSize)
            {
                FSItem.SetEquals(item1, item2);
            }
            else
            {
                Boolean isNewer1 = (file1.LastWriteTime - file2.LastWriteTime).TotalSeconds > 5;
                Boolean isOlder1 = (file2.LastWriteTime - file1.LastWriteTime).TotalSeconds > 5;
                Boolean isBigger1 = file1.Length > file2.Length;
                Boolean isSmaller1 = file1.Length < file2.Length;

                file1.SetAllFlags(false, isNewer1, isOlder1, isBigger1, isSmaller1);

                Boolean isNewer2 = (file2.LastWriteTime - file1.LastWriteTime).TotalSeconds > 5;
                Boolean isOlder2 = (file1.LastWriteTime - file2.LastWriteTime).TotalSeconds > 5;
                Boolean isBigger2 = file2.Length > file1.Length;
                Boolean isSmaller2 = file2.Length < file1.Length;

                file2.SetAllFlags(false, isNewer2, isOlder2, isBigger2, isSmaller2);

                file1.IsChecked = true;
            }
        }

        /// <summary>
        /// Установка статуса выбора только для файлов, требующих синхронизации
        /// </summary>
        /// <param name="root"></param>
        private void SetCheckStatuses(FSDirectory root)
        {
            // В процессе анализа все дочерние файлы отмечены для синхронизации (как следствие, отмечена и родительская папка)
            if (root.IsChecked == true) return;

            if (root.UnequalChildren.Count > 0)
            {
                foreach (FSItem item in root.UnequalChildren)
                {
                    if (forceStop) return;

                    if (item.IsDirectory)
                    {
                        SetCheckStatuses(item as FSDirectory);
                    }
                    else
                    {
                        if (item.IsUnique) item.IsChecked = true;
                    }
                }
            }
            else
            {
                if (root.IsUnique && root.Parent != null) root.IsChecked = true;
            }
        }
    }
}

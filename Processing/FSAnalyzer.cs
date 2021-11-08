using Synchronizer.Log;
using Synchronizer.Model;
using System;
using System.Threading;


namespace Synchronizer.Processing
{
    public class FSAnalyzer : AsyncBase
    {
        public FSAnalyzer(FSTree tree1, FSTree tree2)
        {
            execThread = new Thread(() =>
            {
                DateTime start = DateTime.Now;
                Logger.RaiseLog("Analysis started");

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

        private Int32 RecursiveCalcCount(FSItem item)
        {
            Int32 count = 1;
            foreach (FSItem child in item.Children)
            {
                count += RecursiveCalcCount(child);
            }
            return count;
        }

        private void RecursiveAnalysis(FSItem root1, FSItem root2)
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
                            RecursiveAnalysis(item1, item2);
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
            if (root1.IsDirectory)
            {
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

            Boolean sameDate = file1.LastWriteTime == file2.LastWriteTime;
            Boolean sameSize = file1.Length == file2.Length;

            
            if (sameDate && sameSize)
            {
                FSItem.SetEquals(item1, item2);
            }
            else
            {
                Boolean isNewer1 = file1.LastWriteTime > file2.LastWriteTime;
                Boolean isOlder1 = file1.LastWriteTime < file2.LastWriteTime;
                Boolean isBigger1 = file1.Length > file2.Length;
                Boolean isSmaller1 = file1.Length < file2.Length;

                file1.SetAllFlags(false, isNewer1, isOlder1, isBigger1, isSmaller1);

                Boolean isNewer2 = file2.LastWriteTime > file1.LastWriteTime;
                Boolean isOlder2 = file2.LastWriteTime < file1.LastWriteTime;
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
        private void SetCheckStatuses(FSItem root)
        {
            // Status is specified by childrens'
            if (root.UnequalChildren.Count > 0)
            {
                foreach (FSItem item in root.UnequalChildren)
                {
                    // All children is checked already
                    if (root.IsChecked == true) continue;

                    SetCheckStatuses(item);
                }
            } 
            else
            {
                if (root.IsUnique && root.Parent != null) root.IsChecked = true;
            }
        }
    }
}

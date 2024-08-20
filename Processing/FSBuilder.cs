using System;
using System.IO;
using System.Threading;
using Synchronizer2.Model;
using Synchronizer2.Log;

namespace Synchronizer2.Processing
{
    public class FSBuilder : AsyncBase
    {
        public FSTree Tree { get; private set; }

        public FSBuilder(DirectoryInfo directoryPath)
        {
            Tree = new FSTree(new FSDirectory(directoryPath, null));

            execThread = new Thread(() =>
            {
                DateTime start = DateTime.Now;
                Logger.RaiseLog("Build in progress...");

                RecursiveTreeBuild(Tree.Root, 100);

                if (forceStop)
                {
                    Logger.RaiseError("Build stoped - " + directoryPath.FullName);
                } else
                {
                    Logger.RaiseLog("Build completed  - " + directoryPath.FullName + " (" + (DateTime.Now - start).TotalSeconds + " s)");
                }
            });
        }

        private void RecursiveTreeBuild(FSDirectory root, Double progressInc)
        {
            try
            {
                DirectoryInfo di = root.Info as DirectoryInfo;
                DirectoryInfo[] directories = new DirectoryInfo[0];
                try
                {
                    directories = di.GetDirectories();
                } 
                catch (Exception ex)
                {
                    Logger.RaiseError("Warning: " + ex.Message);
                }
                
                FileInfo[] files = new FileInfo[0];
                try
                {
                    files = di.GetFiles();
                }
                catch (Exception ex)
                {
                    Logger.RaiseError("Warning: " + ex.Message);
                }

                Double progressIncChild = progressInc / (directories.Length + 1);

                Progress += progressIncChild;

                //Process directories
                foreach (DirectoryInfo directory in directories)
                {
                    if (forceStop) return;

                    RecursiveTreeBuild(root.EmbedDirectory(directory), progressIncChild);
                }

                //Process files
                foreach (FileInfo file in files)
                {
                    if (forceStop) return;

                    root.EmbedFile(file);
                }
            }
            catch (Exception ex)
            {
                Logger.RaiseError("Error: " + ex.Message);
                forceStop = true;
            }
        }
    }
}

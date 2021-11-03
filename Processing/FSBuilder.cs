using System;
using System.IO;
using System.Threading;
using Synchronizer.Model;
using Synchronizer.Log;

namespace Synchronizer.Processing
{
    public class FSBuilder : AsyncBase
    {
        public FSDirectory Root { get; private set; }

        public FSBuilder(DirectoryInfo directoryPath)
        {
            Root = new FSDirectory(directoryPath, null);

            execThread = new Thread(() =>
            {
                RecursiveTreeBuild(Root, 100);
            });
        }

        private void RecursiveTreeBuild(FSDirectory root, Double progressInc)
        {
            try
            {
                DirectoryInfo di = root.Info as DirectoryInfo;
                DirectoryInfo[] directories = di.GetDirectories();
                FileInfo[] files = di.GetFiles();
                Double progressIncChild = progressInc / (directories.Length + 1);

                //Process directories
                foreach (DirectoryInfo directory in directories)
                {
                    if (forceStop) return;

                    RecursiveTreeBuild(root.EmbedDirectory(directory), progressIncChild);
                }

                Progress += progressIncChild;

                //Process files
                foreach (FileInfo file in files)
                {
                    if (forceStop) return;

                    root.EmbedFile(file);
                }
            }
            catch (Exception ex)
            {
                Logger.RaiseMessage("Error: " + ex.Message);
                forceStop = true;
            }
        }
    }
}
